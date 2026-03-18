using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Xunit;

using CodeCoach.Domain.Entities;
using CodeCoach.Domain.Enums;
using CodeCoach.Infrastructure.Data;
using CodeCoach.Infrastructure.Data.Repositories;

namespace CodeCoach.Api.Tests.Infrastructure.Repositories;

public class RoomRepositoryTests
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
        var repository = new RoomRepository(dbContext);

        var mentorId = Guid.NewGuid();
        var createdById = Guid.NewGuid();
        var room = new Room("Test Room", "JOIN123", mentorId, createdById);

        var result = await repository.AddAsync(room);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Test Room", result.Name);
    }

    [Fact]
    public async Task GetByJoinCodeAsync_ReturnsRoom_WhenExists()
    {
        using var dbContext = CreateDbContext();
        var repository = new RoomRepository(dbContext);

        var mentorId = Guid.NewGuid();
        var createdById = Guid.NewGuid();
        var room = new Room("Test Room", "JOIN456", mentorId, createdById);
        await repository.AddAsync(room);

        var result = await repository.GetByJoinCodeAsync("JOIN456");

        Assert.NotNull(result);
        Assert.Equal("Test Room", result.Name);
    }

    [Fact]
    public async Task GetByJoinCodeAsync_ReturnsNull_WhenNotExists()
    {
        using var dbContext = CreateDbContext();
        var repository = new RoomRepository(dbContext);

        var result = await repository.GetByJoinCodeAsync("NOTEXIST");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveRoomsByMentorIdAsync_ReturnsOnlyActiveRooms_ForGivenMentor()
    {
        using var dbContext = CreateDbContext();
        var repository = new RoomRepository(dbContext);

        var mentorId = Guid.NewGuid();
        var otherMentorId = Guid.NewGuid();
        var createdById = Guid.NewGuid();

        var activeRoom1 = new Room("Active Room 1", "CODE1", mentorId, createdById);
        var activeRoom2 = new Room("Active Room 2", "CODE2", mentorId, createdById);
        var closedRoom = new Room("Closed Room", "CODE3", mentorId, createdById);

        typeof(Room).GetProperty("Status")!.SetValue(closedRoom, RoomStatus.Closed);

        await repository.AddAsync(activeRoom1);
        await repository.AddAsync(activeRoom2);
        await repository.AddAsync(closedRoom);

        var result = await repository.GetActiveRoomsByMentorIdAsync(mentorId);

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(RoomStatus.Active, r.Status));
    }

    [Fact]
    public async Task UpdateAsync_ModifiesEntity()
    {
        using var dbContext = CreateDbContext();
        var repository = new RoomRepository(dbContext);

        var mentorId = Guid.NewGuid();
        var createdById = Guid.NewGuid();
        var room = new Room("Original Name", "UPD1", mentorId, createdById);
        await repository.AddAsync(room);

        typeof(Room).GetProperty("Name")!.SetValue(room, "Updated Name");
        await repository.UpdateAsync(room);

        var result = await repository.GetByIdAsync(room.Id);
        Assert.Equal("Updated Name", result!.Name);
    }
}
