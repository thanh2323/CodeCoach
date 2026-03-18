using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Moq;

using Xunit;

using CodeCoach.Application.Interfaces;
using CodeCoach.Application.Rooms.Services;
using CodeCoach.Domain.Entities;
using CodeCoach.Domain.Enums;

namespace CodeCoach.Api.Tests.Application.Rooms.Services;

public class RoomJoinServiceTests
{
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRoomParticipantRepository> _roomParticipantRepositoryMock;
    private readonly Mock<IWorkspaceRepository> _workspaceRepositoryMock;
    private readonly RoomJoinService _service;

    public RoomJoinServiceTests()
    {
        _roomRepositoryMock = new Mock<IRoomRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _roomParticipantRepositoryMock = new Mock<IRoomParticipantRepository>();
        _workspaceRepositoryMock = new Mock<IWorkspaceRepository>();

        _service = new RoomJoinService(
            _roomRepositoryMock.Object,
            _userRepositoryMock.Object,
            _roomParticipantRepositoryMock.Object,
            _workspaceRepositoryMock.Object);
    }

    [Fact]
    public async Task JoinAsync_WhenRoomAndUserAreValid_CreatesParticipantAndWorkspace()
    {
        var userId = Guid.NewGuid();
        var mentorId = Guid.NewGuid();
        var room = new Room("Algorithms", "JOIN01", mentorId, mentorId);
        var user = new User("Alice", null, null);
        RoomParticipant? capturedParticipant = null;
        Workspace? capturedWorkspace = null;

        _roomRepositoryMock
            .Setup(mock => mock.GetByJoinCodeAsync("JOIN01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);
        _userRepositoryMock
            .Setup(mock => mock.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _roomParticipantRepositoryMock
            .Setup(mock => mock.GetParticipantAsync(room.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoomParticipant?)null);
        _roomParticipantRepositoryMock
            .Setup(mock => mock.AddAsync(It.IsAny<RoomParticipant>(), It.IsAny<CancellationToken>()))
            .Callback<RoomParticipant, CancellationToken>((participant, _) => capturedParticipant = participant)
            .ReturnsAsync((RoomParticipant participant, CancellationToken _) => participant);
        _workspaceRepositoryMock
            .Setup(mock => mock.AddAsync(It.IsAny<Workspace>(), It.IsAny<CancellationToken>()))
            .Callback<Workspace, CancellationToken>((workspace, _) => capturedWorkspace = workspace)
            .ReturnsAsync((Workspace workspace, CancellationToken _) => workspace);

        var result = await _service.JoinAsync("JOIN01", userId, CancellationToken.None);

        Assert.Equal(room.Id, result.Id);
        Assert.Equal("Algorithms", result.Name);
        Assert.Equal("JOIN01", result.JoinCode);
        Assert.Equal("Active", result.Status);
        Assert.Equal("Broadcast", result.CurrentMode);

        Assert.NotNull(capturedParticipant);
        Assert.Equal(room.Id, capturedParticipant!.RoomId);
        Assert.Equal(userId, capturedParticipant.UserId);
        Assert.Equal(RoomRole.Student, capturedParticipant.Role);

        Assert.NotNull(capturedWorkspace);
        Assert.Equal(room.Id, capturedWorkspace!.RoomId);
        Assert.Equal(userId, capturedWorkspace.UserId);
        Assert.Equal("csharp", capturedWorkspace.Language);
        Assert.Equal(string.Empty, capturedWorkspace.SourceCode);
    }

    [Fact]
    public async Task JoinAsync_WhenRoomDoesNotExist_ThrowsKeyNotFoundException()
    {
        _roomRepositoryMock
            .Setup(mock => mock.GetByJoinCodeAsync("MISSING", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var action = () => _service.JoinAsync("MISSING", Guid.NewGuid(), CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    [Fact]
    public async Task JoinAsync_WhenUserDoesNotExist_ThrowsKeyNotFoundException()
    {
        var mentorId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var room = new Room("Algorithms", "JOIN01", mentorId, mentorId);

        _roomRepositoryMock
            .Setup(mock => mock.GetByJoinCodeAsync("JOIN01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);
        _userRepositoryMock
            .Setup(mock => mock.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var action = () => _service.JoinAsync("JOIN01", userId, CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    [Fact]
    public async Task JoinAsync_WhenRoomIsClosed_ThrowsInvalidOperationException()
    {
        var mentorId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var room = new Room("Algorithms", "JOIN01", mentorId, mentorId);
        var user = new User("Alice", null, null);

        room.Close();

        _roomRepositoryMock
            .Setup(mock => mock.GetByJoinCodeAsync("JOIN01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);
        _userRepositoryMock
            .Setup(mock => mock.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var action = () => _service.JoinAsync("JOIN01", userId, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }

    [Fact]
    public async Task JoinAsync_WhenUserAlreadyJoined_ThrowsInvalidOperationException()
    {
        var mentorId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var room = new Room("Algorithms", "JOIN01", mentorId, mentorId);
        var user = new User("Alice", null, null);
        var participant = new RoomParticipant(room.Id, userId, RoomRole.Student);

        _roomRepositoryMock
            .Setup(mock => mock.GetByJoinCodeAsync("JOIN01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);
        _userRepositoryMock
            .Setup(mock => mock.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _roomParticipantRepositoryMock
            .Setup(mock => mock.GetParticipantAsync(room.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(participant);

        var action = () => _service.JoinAsync("JOIN01", userId, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);
    }
}
