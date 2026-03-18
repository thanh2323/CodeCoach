using System;

using MediatR;

namespace CodeCoach.Application.Workspaces.Commands.SaveWorkspaceSnapshot;

public record SaveWorkspaceSnapshotCommand(
    Guid RoomId,
    Guid UserId,
    string Language,
    string SourceCode) : IRequest<Unit>;
