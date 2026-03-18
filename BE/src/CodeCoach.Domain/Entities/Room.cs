using System;

using CodeCoach.Domain.Enums;

namespace CodeCoach.Domain.Entities;

public class Room
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string JoinCode { get; private set; }
    public Guid MentorId { get; private set; }
    public Guid CreatedById { get; private set; }
    public RoomStatus Status { get; private set; }
    public RoomMode CurrentMode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    public Room(string name, string joinCode, Guid mentorId, Guid createdById)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(joinCode))
        {
            throw new ArgumentException("JoinCode is required.", nameof(joinCode));
        }

        if (mentorId == Guid.Empty)
        {
            throw new ArgumentException("MentorId is required.", nameof(mentorId));
        }

        if (createdById == Guid.Empty)
        {
            throw new ArgumentException("CreatedById is required.", nameof(createdById));
        }

        Id = Guid.NewGuid();
        Name = name;
        JoinCode = joinCode;
        MentorId = mentorId;
        CreatedById = createdById;
        Status = RoomStatus.Active;
        CurrentMode = RoomMode.Broadcast;
        CreatedAt = DateTime.UtcNow;
    }

    public void EnsureActive()
    {
        if (Status != RoomStatus.Active)
        {
            throw new InvalidOperationException("Room is not active.");
        }
    }

    public void Close()
    {
        if (Status == RoomStatus.Closed)
        {
            return;
        }

        Status = RoomStatus.Closed;
        ClosedAt = DateTime.UtcNow;
    }
}
