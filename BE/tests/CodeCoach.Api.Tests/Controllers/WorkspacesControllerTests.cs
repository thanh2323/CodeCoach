using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using Xunit;

using CodeCoach.Api.Contracts.Workspaces;
using CodeCoach.Api.Controllers;
using CodeCoach.Application.DTOs;
using CodeCoach.Application.Workspaces.Commands.SaveWorkspaceSnapshot;
using CodeCoach.Application.Workspaces.Queries.GetWorkspace;

namespace CodeCoach.Api.Tests.Controllers;

public class WorkspacesControllerTests
{
    [Fact]
    public async Task GetAsync_ReturnsOk_WhenWorkspaceExists()
    {
        var sender = new Mock<ISender>();
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var workspace = new WorkspaceDto(
            Guid.NewGuid(),
            roomId,
            userId,
            "csharp",
            "Console.WriteLine();",
            DateTime.UtcNow);

        sender.Setup(mock => mock.Send(
                It.Is<GetWorkspaceQuery>(query => query.RoomId == roomId && query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        var controller = CreateController(sender.Object, userId);

        var result = await controller.GetAsync(roomId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(workspace, okResult.Value);
    }

    [Fact]
    public async Task GetAsync_ReturnsNotFound_WhenWorkspaceMissing()
    {
        var sender = new Mock<ISender>();
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        sender.Setup(mock => mock.Send(
                It.Is<GetWorkspaceQuery>(query => query.RoomId == roomId && query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkspaceDto?)null);

        var controller = CreateController(sender.Object, userId);

        var result = await controller.GetAsync(roomId, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task SaveSnapshotAsync_SendsCommand_AndReturnsNoContent()
    {
        var sender = new Mock<ISender>();
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new SaveWorkspaceSnapshotRequest("python", "print('hello')");

        sender.Setup(mock => mock.Send(
                It.Is<SaveWorkspaceSnapshotCommand>(command =>
                    command.RoomId == roomId &&
                    command.UserId == userId &&
                    command.Language == "python" &&
                    command.SourceCode == "print('hello')"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var controller = CreateController(sender.Object, userId);

        var result = await controller.SaveSnapshotAsync(roomId, request, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    private static WorkspacesController CreateController(ISender sender, Guid userId)
    {
        var controller = new WorkspacesController(sender)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                        },
                        "TestAuth"))
                }
            }
        };

        return controller;
    }
}
