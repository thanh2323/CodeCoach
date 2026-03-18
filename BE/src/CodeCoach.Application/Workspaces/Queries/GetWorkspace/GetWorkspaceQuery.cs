using System;

using MediatR;

using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Workspaces.Queries.GetWorkspace;

public record GetWorkspaceQuery(Guid RoomId, Guid UserId) : IRequest<WorkspaceDto?>;
