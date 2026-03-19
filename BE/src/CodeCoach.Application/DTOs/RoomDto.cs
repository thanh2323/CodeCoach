using System;

namespace CodeCoach.Application.DTOs;

public record RoomDto(Guid Id, string Name, string JoinCode, Guid MentorId, string Status);
