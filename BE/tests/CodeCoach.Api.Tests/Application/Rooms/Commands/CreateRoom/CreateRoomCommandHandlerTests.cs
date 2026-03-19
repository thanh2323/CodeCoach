using System;
using System.Threading;
using System.Threading.Tasks;

using Moq;

using CodeCoach.Application.DTOs;
using CodeCoach.Application.Features.Rooms.Commands.CreateRoom;
using CodeCoach.Application.Features.Rooms.Services;

namespace CodeCoach.Api.Tests.Application.Rooms.Commands.CreateRoom;

public class CreateRoomCommandHandlerTests
{
    private readonly Mock<ICreateRoomService> _createRoomServiceMock;
    private readonly CreateRoomCommandHandler _handler;

    public CreateRoomCommandHandlerTests()
    {
        _createRoomServiceMock = new Mock<ICreateRoomService>();
        _handler = new CreateRoomCommandHandler(_createRoomServiceMock.Object);
    }

    [Fact]
    public async Task Handle_DelegatesToCreateRoomService_AndReturnsResult()
    {
        var userId = Guid.NewGuid();
        var expected = new RoomDto(Guid.NewGuid(), "Test Room", "ABC123", userId, "Active");

        _createRoomServiceMock
            .Setup(mock => mock.CreateAsync("Test Room", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(new CreateRoomCommand("Test Room", userId), CancellationToken.None);

        Assert.Equal(expected, result);
        _createRoomServiceMock.Verify(
            mock => mock.CreateAsync("Test Room", userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
