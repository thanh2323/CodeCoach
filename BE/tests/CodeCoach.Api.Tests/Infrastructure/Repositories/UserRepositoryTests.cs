using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Xunit;

using CodeCoach.Domain.Entities;
using CodeCoach.Infrastructure.Data;
using CodeCoach.Infrastructure.Data.Repositories;

namespace CodeCoach.Api.Tests.Infrastructure.Repositories;

public class UserRepositoryTests
{
    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_PersistsEntity_ReturnsEntityWithId()
    {
        using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);

        var user = new User("Alice", "alice@example.com", "hashed-password");

        var result = await repository.AddAsync(user);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Alice", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEntity_WhenExists()
    {
        using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);

        var user = new User("Bob", "bob@example.com", "hashed-password");
        await repository.AddAsync(user);

        var result = await repository.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal("Bob", result.Name);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsEntity_WhenEmailMatchesIgnoringCase()
    {
        using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);

        var user = new User("Bob", "bob@example.com", "hashed-password");
        await repository.AddAsync(user);

        var result = await repository.GetByEmailAsync("BOB@example.com");

        Assert.NotNull(result);
        Assert.Equal(user.Id, result!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}
