using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Xunit;

using CodeCoach.Domain.Entities;
using CodeCoach.Infrastructure.Data;
using CodeCoach.Infrastructure.Data.Repositories;

namespace CodeCoach.Api.Tests.Infrastructure.Repositories;

public class WorkspaceRepositoryTests
{
    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_PersistsWorkspace()
    {
        using var dbContext = CreateDbContext();
        var repository = new WorkspaceRepository(dbContext);

        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var workspace = new Workspace(roomId, userId, "javascript", "console.log('hello');");

        var result = await repository.AddAsync(workspace);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(roomId, result.RoomId);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task GetWorkspaceAsync_ReturnsWorkspace_ForStudentWorkspace()
    {
        using var dbContext = CreateDbContext();
        var repository = new WorkspaceRepository(dbContext);

        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var workspace = new Workspace(roomId, userId, "python", "print('hello')");
        await repository.AddAsync(workspace);

        var result = await repository.GetWorkspaceAsync(roomId, userId);

        Assert.NotNull(result);
        Assert.Equal("python", result.Language);
    }

    [Fact]
    public async Task GetWorkspaceAsync_ReturnsWorkspace_ForBroadcastWorkspace()
    {
        using var dbContext = CreateDbContext();
        var repository = new WorkspaceRepository(dbContext);

        var roomId = Guid.NewGuid();
        var workspace = new Workspace(roomId, null, "csharp", "Console.WriteLine();");
        await repository.AddAsync(workspace);

        var result = await repository.GetWorkspaceAsync(roomId, null);

        Assert.NotNull(result);
        Assert.Null(result.UserId);
    }

    [Fact]
    public async Task GetWorkspaceAsync_ReturnsNull_WhenNotExists()
    {
        using var dbContext = CreateDbContext();
        var repository = new WorkspaceRepository(dbContext);

        var result = await repository.GetWorkspaceAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesWorkspace()
    {
        using var dbContext = CreateDbContext();
        var repository = new WorkspaceRepository(dbContext);

        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var workspace = new Workspace(roomId, userId, "javascript", "original");
        await repository.AddAsync(workspace);

        typeof(Workspace).GetProperty("SourceCode")!.SetValue(workspace, "updated code");
        await repository.UpdateAsync(workspace);

        var result = await repository.GetWorkspaceAsync(roomId, userId);
        Assert.Equal("updated code", result!.SourceCode);
    }
}
