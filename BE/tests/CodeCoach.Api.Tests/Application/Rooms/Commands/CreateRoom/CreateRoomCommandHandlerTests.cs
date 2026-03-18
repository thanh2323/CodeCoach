using System;
using System.Threading;
using System.Threading.Tasks;

using Moq;

using Xunit;

using CodeCoach.Application.DTOs;
using CodeCoach.Application.Interfaces;
using CodeCoach.Application.Rooms.Commands.CreateRoom;
using CodeCoach.Application.Abstractions;
using CodeCoach.Domain.Entities;

namespace CodeCoach.Api.Tests.Application.Rooms.Commands.CreateRoom;

public class CreateRoomCommandHandlerTests
{
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IJoinCodeGenerator> _joinCodeGeneratorMock;
    private readonly CreateRoomCommandHandler _handler;

    public CreateRoomCommandHandlerTests()
    {
        _roomRepositoryMock = new Mock<IRoomRepository>();
        _joinCodeGeneratorMock = new Mock<IJoinCodeGenerator>();
        _handler = new CreateRoomCommandHandler(
            _roomRepositoryMock.Object,
            _joinCodeGeneratorMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesRoom_ReturnsRoomDto()
    {
        var mentorId = Guid.NewGuid();
        var command = new CreateRoomCommand("Test Room", mentorId);

        _joinCodeGeneratorMock.Setup(x => x.Generate()).Returns("ABC123");
        _roomRepositoryMock
            .Setup(x => x.GetByJoinCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);
        _roomRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Room>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room room, CancellationToken _) => room);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Test Room", result.Name);
        Assert.Equal(mentorId, result.MentorId);
        Assert.Equal("ABC123", result.JoinCode);
    }

    [Fact]
    public async Task Handle_WhenJoinCodeExists_RetriesUntilUnique()
    {
        var mentorId = Guid.NewGuid();
        var command = new CreateRoomCommand("Test Room", mentorId);

        _joinCodeGeneratorMock.SetupSequence(x => x.Generate())
            .Returns("DUPE01")
            .Returns("UNIQUE");

        var existingRoom = new Room("Existing", "DUPE01", Guid.NewGuid(), Guid.NewGuid());
        _roomRepositoryMock
            .SetupSequence(x => x.GetByJoinCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRoom)
            .ReturnsAsync((Room?)null);

        _roomRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Room>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room room, CancellationToken _) => room);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("UNIQUE", result.JoinCode);
    }
}
