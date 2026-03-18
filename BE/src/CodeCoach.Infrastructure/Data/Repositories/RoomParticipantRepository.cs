using Microsoft.EntityFrameworkCore;

using CodeCoach.Domain.Entities;
using CodeCoach.Application.Interfaces;
using CodeCoach.Infrastructure.Data;

namespace CodeCoach.Infrastructure.Data.Repositories;

public class RoomParticipantRepository : IRoomParticipantRepository
{
    private readonly ApplicationDbContext _dbContext;

    public RoomParticipantRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RoomParticipant?> GetParticipantAsync(
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.RoomParticipants
            .AsNoTracking()
            .FirstOrDefaultAsync(
                participant => participant.RoomId == roomId && participant.UserId == userId,
                cancellationToken);
    }

    public Task<List<RoomParticipant>> GetParticipantsByRoomIdAsync(
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.RoomParticipants
            .AsNoTracking()
            .Where(participant => participant.RoomId == roomId)
            .OrderBy(participant => participant.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<RoomParticipant> AddAsync(
        RoomParticipant participant,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.RoomParticipants.AddAsync(participant, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return participant;
    }

    public async Task UpdateAsync(
        RoomParticipant participant,
        CancellationToken cancellationToken = default)
    {
        _dbContext.RoomParticipants.Update(participant);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
