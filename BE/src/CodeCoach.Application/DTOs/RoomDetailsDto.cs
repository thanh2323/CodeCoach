using System;

namespace CodeCoach.Application.DTOs;

public record RoomDetailsDto(
    Guid Id,
    string Name,
    string JoinCode,
    Guid MentorId,
    string Status,
    string CurrentMode,
    Guid ParticipantId,
    Guid UserId,
    Guid WorkspaceId,
    string Language,
    string SourceCode);
