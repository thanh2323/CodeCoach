using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using CodeCoach.Domain.Entities;
using CodeCoach.Infrastructure.Data;
using CodeCoach.Infrastructure.Data.Repositories;
using CodeCoach.Infrastructure.Transactions;

namespace CodeCoach.Api.Tests.Infrastructure.Transactions;

public class TransactionManagerTests
{
    [Fact]
    public async Task ExecuteAsync_RollsBackTrackedChanges_WhenOperationThrows()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new ApplicationDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var mentor = new User("Mentor", "mentor@example.com", "hashed-password");
        dbContext.Users.Add(mentor);
        await dbContext.SaveChangesAsync();

        var roomRepository = new RoomRepository(dbContext);
        var transactionManager = new TransactionManager(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            transactionManager.ExecuteAsync(async cancellationToken =>
            {
                await roomRepository.AddAsync(
                    new Room("Algorithms", "JOIN01", mentor.Id, mentor.Id),
                    cancellationToken);

                throw new InvalidOperationException("boom");
            }, CancellationToken.None));

        var roomCount = await dbContext.Rooms.CountAsync();
        Assert.Equal(0, roomCount);
    }
}
