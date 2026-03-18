using Microsoft.EntityFrameworkCore;

using CodeCoach.Domain.Entities;
using CodeCoach.Domain.Enums;
using CodeCoach.Application.Interfaces;
using CodeCoach.Infrastructure.Data;

namespace CodeCoach.Infrastructure.Data.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly ApplicationDbContext _dbContext;

    public RoomRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(room => room.Id == id, cancellationToken);
    }

    public Task<Room?> GetByJoinCodeAsync(string joinCode, CancellationToken cancellationToken = default)
    {
        return _dbContext.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(room => room.JoinCode == joinCode, cancellationToken);
    }

    public Task<List<Room>> GetActiveRoomsByMentorIdAsync(
        Guid mentorId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Rooms
            .AsNoTracking()
            .Where(room => room.MentorId == mentorId && room.Status == RoomStatus.Active)
            .OrderByDescending(room => room.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Room> AddAsync(Room room, CancellationToken cancellationToken = default)
    {
        await _dbContext.Rooms.AddAsync(room, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return room;
    }

    public async Task UpdateAsync(Room room, CancellationToken cancellationToken = default)
    {
        _dbContext.Rooms.Update(room);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
