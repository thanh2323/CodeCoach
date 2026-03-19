using System;

namespace CodeCoach.Application.DTOs;

public record WorkspaceDto(
    Guid Id,
    Guid RoomId,
    Guid? UserId,
    string Language,
    string SourceCode,
    DateTime UpdatedAt);
