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

public class RoomParticipantRepositoryTests
{
    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_PersistsParticipant()
    {
        using var dbContext = CreateDbContext();
        var repository = new RoomParticipantRepository(dbContext);

        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var participant = new RoomParticipant(roomId, userId, RoomRole.Student);

        var result = await repository.AddAsync(participant);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(roomId, result.RoomId);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task GetParticipantAsync_ReturnsParticipant_ByRoomIdAndUserId()
    {
        using var dbContext = CreateDbContext();
        var repository = new RoomParticipantRepository(dbContext);

        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var participant = new RoomParticipant(roomId, userId, RoomRole.Student);
        await repository.AddAsync(participant);

        var result = await repository.GetParticipantAsync(roomId, userId);

        Assert.NotNull(result);
        Assert.Equal(RoomRole.Student, result.Role);
    }

    [Fact]
    public async Task GetParticipantAsync_ReturnsNull_WhenNotExists()
    {
        using var dbContext = CreateDbContext();
        var repository = new RoomParticipantRepository(dbContext);

        var result = await repository.GetParticipantAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetParticipantsByRoomIdAsync_ReturnsParticipants_OrderedByJoinedAt()
    {
        using var dbContext = CreateDbContext();
        var repository = new RoomParticipantRepository(dbContext);

        var roomId = Guid.NewGuid();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var participant1 = new RoomParticipant(roomId, userId1, RoomRole.Student);
        var participant2 = new RoomParticipant(roomId, userId2, RoomRole.Student);

        await repository.AddAsync(participant1);
        await Task.Delay(10);
        await repository.AddAsync(participant2);

        var result = await repository.GetParticipantsByRoomIdAsync(roomId);

        Assert.Equal(2, result.Count);
        Assert.Equal(userId1, result[0].UserId);
        Assert.Equal(userId2, result[1].UserId);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesParticipant()
    {
        using var dbContext = CreateDbContext();
        var repository = new RoomParticipantRepository(dbContext);

        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var participant = new RoomParticipant(roomId, userId, RoomRole.Student);
        await repository.AddAsync(participant);

        dbContext.Entry(participant).State = EntityState.Modified;
        await repository.UpdateAsync(participant);

        var result = await repository.GetParticipantAsync(roomId, userId);
        Assert.NotNull(result);
    }
}
