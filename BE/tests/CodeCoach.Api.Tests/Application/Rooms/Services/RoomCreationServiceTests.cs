using Moq;

using CodeCoach.Application.Abstractions;
using CodeCoach.Application.Exceptions;
using CodeCoach.Application.Interfaces;
using CodeCoach.Application.Features.Rooms.Services;
using CodeCoach.Domain.Entities;
using CodeCoach.Domain.Enums;

namespace CodeCoach.Api.Tests.Application.Rooms.Services;

public class RoomCreationServiceTests
{
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRoomParticipantRepository> _roomParticipantRepositoryMock;
    private readonly Mock<IWorkspaceRepository> _workspaceRepositoryMock;
    private readonly Mock<ITransactionManager> _transactionManagerMock;
    private readonly RoomCreationService _service;

    public RoomCreationServiceTests()
    {
        _roomRepositoryMock = new Mock<IRoomRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _roomParticipantRepositoryMock = new Mock<IRoomParticipantRepository>();
        _workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
        _transactionManagerMock = new Mock<ITransactionManager>();

        _transactionManagerMock
            .Setup(mock => mock.ExecuteAsync<CodeCoach.Application.DTOs.RoomDto>(
                It.IsAny<Func<CancellationToken, Task<CodeCoach.Application.DTOs.RoomDto>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task<CodeCoach.Application.DTOs.RoomDto>> operation, CancellationToken ct) =>
                operation(ct));

        _service = new RoomCreationService(
            _roomRepositoryMock.Object,
            _userRepositoryMock.Object,
            _roomParticipantRepositoryMock.Object,
            _workspaceRepositoryMock.Object,
            _transactionManagerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenUserExists_CreatesRoomMentorParticipantAndWorkspace()
    {
        var userId = Guid.NewGuid();
        var user = new User("Mentor", "mentor@example.com", "hashed-password");
        Room? capturedRoom = null;
        RoomParticipant? capturedParticipant = null;
        Workspace? capturedWorkspace = null;

        _userRepositoryMock
            .Setup(mock => mock.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _roomRepositoryMock
            .Setup(mock => mock.GetByJoinCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);
        _roomRepositoryMock
            .Setup(mock => mock.AddAsync(It.IsAny<Room>(), It.IsAny<CancellationToken>()))
            .Callback<Room, CancellationToken>((room, _) => capturedRoom = room)
            .ReturnsAsync((Room room, CancellationToken _) => room);
        _roomParticipantRepositoryMock
            .Setup(mock => mock.AddAsync(It.IsAny<RoomParticipant>(), It.IsAny<CancellationToken>()))
            .Callback<RoomParticipant, CancellationToken>((participant, _) => capturedParticipant = participant)
            .ReturnsAsync((RoomParticipant participant, CancellationToken _) => participant);
        _workspaceRepositoryMock
            .Setup(mock => mock.AddAsync(It.IsAny<Workspace>(), It.IsAny<CancellationToken>()))
            .Callback<Workspace, CancellationToken>((workspace, _) => capturedWorkspace = workspace)
            .ReturnsAsync((Workspace workspace, CancellationToken _) => workspace);

        var result = await _service.CreateAsync("Algorithms", userId, CancellationToken.None);

        Assert.Equal("Algorithms", result.Name);
        Assert.Equal(userId, result.MentorId);
        Assert.NotNull(result.JoinCode);
        Assert.Equal(6, result.JoinCode.Length);

        Assert.NotNull(capturedRoom);
        Assert.Equal(userId, capturedRoom!.MentorId);
        Assert.Equal(userId, capturedRoom.CreatedById);

        Assert.NotNull(capturedParticipant);
        Assert.Equal(RoomRole.Mentor, capturedParticipant!.Role);
        Assert.Equal(userId, capturedParticipant.UserId);

        Assert.NotNull(capturedWorkspace);
        Assert.Equal(userId, capturedWorkspace!.UserId);
        Assert.Equal("csharp", capturedWorkspace.Language);
        Assert.Equal(string.Empty, capturedWorkspace.SourceCode);
    }

    [Fact]
    public async Task CreateAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        _userRepositoryMock
            .Setup(mock => mock.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var action = () => _service.CreateAsync("Algorithms", Guid.NewGuid(), CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(action);
    }

    [Fact]
    public async Task CreateAsync_WhenJoinCodeExists_RetriesWithNewCode()
    {
        var userId = Guid.NewGuid();
        var user = new User("Mentor", "mentor@example.com", "hashed-password");
        var existingJoinCode = "AAAAAA";
        Room? capturedRoom = null;

        _userRepositoryMock
            .Setup(mock => mock.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _roomRepositoryMock
            .SetupSequence(mock => mock.GetByJoinCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Room("Existing", existingJoinCode, userId, userId))
            .ReturnsAsync((Room?)null);
        _roomRepositoryMock
            .Setup(mock => mock.AddAsync(It.IsAny<Room>(), It.IsAny<CancellationToken>()))
            .Callback<Room, CancellationToken>((room, _) => capturedRoom = room)
            .ReturnsAsync((Room room, CancellationToken _) => room);
        _roomParticipantRepositoryMock
            .Setup(mock => mock.AddAsync(It.IsAny<RoomParticipant>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoomParticipant participant, CancellationToken _) => participant);
        _workspaceRepositoryMock
            .Setup(mock => mock.AddAsync(It.IsAny<Workspace>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Workspace workspace, CancellationToken _) => workspace);

        var result = await _service.CreateAsync("Algorithms", userId, CancellationToken.None);

        Assert.NotNull(capturedRoom);
        Assert.NotEqual(existingJoinCode, capturedRoom!.JoinCode);
        _roomRepositoryMock.Verify(
            mock => mock.GetByJoinCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}
