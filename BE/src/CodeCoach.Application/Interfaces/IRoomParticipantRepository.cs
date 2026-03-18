using System;

using CodeCoach.Domain.Entities;

namespace CodeCoach.Application.Interfaces;

public interface IRoomParticipantRepository
{
    Task<RoomParticipant?> GetParticipantAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<RoomParticipant>> GetParticipantsByRoomIdAsync(Guid roomId, CancellationToken cancellationToken = default);
    Task<RoomParticipant> AddAsync(RoomParticipant participant, CancellationToken cancellationToken = default);
    Task UpdateAsync(RoomParticipant participant, CancellationToken cancellationToken = default);
}
