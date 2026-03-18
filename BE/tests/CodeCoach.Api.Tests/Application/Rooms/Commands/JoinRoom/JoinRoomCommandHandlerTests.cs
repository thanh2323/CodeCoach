using System;
using System.Threading;
using System.Threading.Tasks;

using Moq;

using Xunit;

using CodeCoach.Application.DTOs;
using CodeCoach.Application.Rooms.Commands.JoinRoom;
using CodeCoach.Application.Rooms.Services;

namespace CodeCoach.Api.Tests.Application.Rooms.Commands.JoinRoom;

public class JoinRoomCommandHandlerTests
{
    private readonly Mock<IJoinRoomService> _joinRoomServiceMock;
    private readonly JoinRoomCommandHandler _handler;

    public JoinRoomCommandHandlerTests()
    {
        _joinRoomServiceMock = new Mock<IJoinRoomService>();
        _handler = new JoinRoomCommandHandler(_joinRoomServiceMock.Object);
    }

    [Fact]
    public async Task Handle_DelegatesToJoinRoomService_AndReturnsResult()
    {
        var userId = Guid.NewGuid();
        var expected = new RoomDetailsDto(
            Guid.NewGuid(),
            "Algorithms",
            "JOIN01",
            Guid.NewGuid(),
            "Active",
            "Broadcast",
            Guid.NewGuid(),
            userId,
            Guid.NewGuid(),
            "csharp",
            string.Empty);

        _joinRoomServiceMock
            .Setup(mock => mock.JoinAsync("JOIN01", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(new JoinRoomCommand("JOIN01", userId), CancellationToken.None);

        Assert.Equal(expected, result);
        _joinRoomServiceMock.Verify(
            mock => mock.JoinAsync("JOIN01", userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
