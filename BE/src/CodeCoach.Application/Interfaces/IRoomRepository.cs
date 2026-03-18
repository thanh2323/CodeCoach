using System;

using CodeCoach.Domain.Entities;

namespace CodeCoach.Application.Interfaces;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Room?> GetByJoinCodeAsync(string joinCode, CancellationToken cancellationToken = default);
    Task<List<Room>> GetActiveRoomsByMentorIdAsync(Guid mentorId, CancellationToken cancellationToken = default);
    Task<Room> AddAsync(Room room, CancellationToken cancellationToken = default);
    Task UpdateAsync(Room room, CancellationToken cancellationToken = default);
}
