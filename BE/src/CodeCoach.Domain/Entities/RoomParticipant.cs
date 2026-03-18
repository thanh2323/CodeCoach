using System;

using CodeCoach.Domain.Enums;

namespace CodeCoach.Domain.Entities;

public class RoomParticipant
{
    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public Guid UserId { get; private set; }
    public RoomRole Role { get; private set; }
    public ParticipantStatus Status { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LastActiveAt { get; private set; }

    public RoomParticipant(Guid roomId, Guid userId, RoomRole role)
    {
        if (roomId == Guid.Empty)
        {
            throw new ArgumentException("RoomId is required.", nameof(roomId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        Id = Guid.NewGuid();
        RoomId = roomId;
        UserId = userId;
        Role = role;
        Status = ParticipantStatus.Active;
        JoinedAt = DateTime.UtcNow;
    }
}
