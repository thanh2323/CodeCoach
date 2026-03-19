using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Moq;

using CodeCoach.Application.Exceptions;
using CodeCoach.Application.Interfaces;
using CodeCoach.Application.Features.Workspaces.Commands.SaveWorkspaceSnapshot;
using CodeCoach.Domain.Entities;

namespace CodeCoach.Api.Tests.Application.Workspaces.Commands.SaveWorkspaceSnapshot;

public class SaveWorkspaceSnapshotCommandHandlerTests
{
    private readonly Mock<IWorkspaceRepository> _workspaceRepositoryMock;
    private readonly SaveWorkspaceSnapshotCommandHandler _handler;

    public SaveWorkspaceSnapshotCommandHandlerTests()
    {
        _workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
        _handler = new SaveWorkspaceSnapshotCommandHandler(_workspaceRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_UpdatesWorkspaceSnapshot_WhenWorkspaceExists()
    {
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var workspace = new Workspace(roomId, userId, "csharp", "before");

        _workspaceRepositoryMock
            .Setup(mock => mock.GetWorkspaceAsync(roomId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        await _handler.Handle(
            new SaveWorkspaceSnapshotCommand(roomId, userId, "python", "print('after')"),
            CancellationToken.None);

        Assert.Equal("python", workspace.Language);
        Assert.Equal("print('after')", workspace.SourceCode);
        _workspaceRepositoryMock.Verify(
            mock => mock.UpdateAsync(workspace, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenWorkspaceDoesNotExist()
    {
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _workspaceRepositoryMock
            .Setup(mock => mock.GetWorkspaceAsync(roomId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Workspace?)null);

        var action = () => _handler.Handle(
            new SaveWorkspaceSnapshotCommand(roomId, userId, "python", "print('after')"),
            CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(action);
    }
}
