using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Moq;

using Xunit;

using CodeCoach.Api.Contracts.Rooms;
using CodeCoach.Api.Controllers;
using CodeCoach.Application.DTOs;
using CodeCoach.Application.Rooms.Commands.CreateRoom;
using CodeCoach.Application.Rooms.Commands.JoinRoom;
using CodeCoach.Application.Rooms.Queries.GetRoomByJoinCode;

namespace CodeCoach.Api.Tests.Controllers;

public class RoomsControllerTests
{
    [Fact]
    public async Task CreateAsync_SendsCommand_AndReturnsCreatedAtAction()
    {
        var sender = new Mock<ISender>();
        var mentorId = Guid.NewGuid();
        var room = new RoomDto(Guid.NewGuid(), "Test Room", "ABC123", mentorId, "Active");

        sender.Setup(mock => mock.Send(
                It.IsAny<CreateRoomCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var controller = new RoomsController(sender.Object);
        var request = new CreateRoomRequest("Test Room", mentorId);

        var result = await controller.CreateAsync(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(RoomsController.GetByJoinCodeAsync), createdResult.ActionName);
        Assert.Equal(room, createdResult.Value);
    }

    [Fact]
    public async Task GetByJoinCodeAsync_ReturnsOk_WhenRoomExists()
    {
        var sender = new Mock<ISender>();
        var room = new RoomDto(Guid.NewGuid(), "Test Room", "ABC123", Guid.NewGuid(), "Active");

        sender.Setup(mock => mock.Send(
                It.IsAny<GetRoomByJoinCodeQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var controller = new RoomsController(sender.Object);

        var result = await controller.GetByJoinCodeAsync("ABC123", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(room, okResult.Value);
    }

    [Fact]
    public async Task GetByJoinCodeAsync_ReturnsNotFound_WhenRoomMissing()
    {
        var sender = new Mock<ISender>();

        sender.Setup(mock => mock.Send(
                It.IsAny<GetRoomByJoinCodeQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoomDto?)null);

        var controller = new RoomsController(sender.Object);

        var result = await controller.GetByJoinCodeAsync("MISSING", CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task JoinAsync_SendsCommand_AndReturnsOk()
    {
        var sender = new Mock<ISender>();
        var userId = Guid.NewGuid();
        var room = new RoomDetailsDto(
            Guid.NewGuid(),
            "Test Room",
            "ABC123",
            Guid.NewGuid(),
            "Active",
            "Broadcast",
            Guid.NewGuid(),
            userId,
            Guid.NewGuid(),
            "csharp",
            string.Empty);

        sender.Setup(mock => mock.Send(
                It.Is<JoinRoomCommand>(command => command.JoinCode == "ABC123" && command.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var controller = new RoomsController(sender.Object);
        var request = new JoinRoomRequest(userId);

        var result = await controller.JoinAsync("ABC123", request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(room, okResult.Value);
    }
}
