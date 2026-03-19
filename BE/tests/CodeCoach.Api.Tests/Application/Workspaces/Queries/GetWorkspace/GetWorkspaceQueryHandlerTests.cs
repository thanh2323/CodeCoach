using System;
using System.Threading;
using System.Threading.Tasks;

using Moq;

using Xunit;

using CodeCoach.Application.DTOs;
using CodeCoach.Application.Interfaces;
using CodeCoach.Application.Workspaces.Queries.GetWorkspace;
using CodeCoach.Domain.Entities;

namespace CodeCoach.Api.Tests.Application.Workspaces.Queries.GetWorkspace;

public class GetWorkspaceQueryHandlerTests
{
    private readonly Mock<IWorkspaceRepository> _workspaceRepositoryMock;
    private readonly GetWorkspaceQueryHandler _handler;

    public GetWorkspaceQueryHandlerTests()
    {
        _workspaceRepositoryMock = new Mock<IWorkspaceRepository>();
        _handler = new GetWorkspaceQueryHandler(_workspaceRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsWorkspaceDto_WhenWorkspaceExists()
    {
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var workspace = new Workspace(roomId, userId, "csharp", "Console.WriteLine();");

        _workspaceRepositoryMock
            .Setup(mock => mock.GetWorkspaceAsync(roomId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        var result = await _handler.Handle(new GetWorkspaceQuery(roomId, userId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(workspace.Id, result!.Id);
        Assert.Equal("csharp", result.Language);
        Assert.Equal("Console.WriteLine();", result.SourceCode);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenWorkspaceDoesNotExist()
    {
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _workspaceRepositoryMock
            .Setup(mock => mock.GetWorkspaceAsync(roomId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Workspace?)null);

        var result = await _handler.Handle(new GetWorkspaceQuery(roomId, userId), CancellationToken.None);

        Assert.Null(result);
    }
}
