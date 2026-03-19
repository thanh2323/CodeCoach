using MediatR;

using CodeCoach.Application.DTOs;
using CodeCoach.Application.Interfaces;

namespace CodeCoach.Application.Workspaces.Queries.GetWorkspace;

public class GetWorkspaceQueryHandler : IRequestHandler<GetWorkspaceQuery, WorkspaceDto?>
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public GetWorkspaceQueryHandler(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public async Task<WorkspaceDto?> Handle(GetWorkspaceQuery request, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetWorkspaceAsync(
            request.RoomId,
            request.UserId,
            cancellationToken);

        if (workspace is null)
        {
            return null;
        }

        return new WorkspaceDto(
            workspace.Id,
            workspace.RoomId,
            workspace.UserId,
            workspace.Language,
            workspace.SourceCode,
            workspace.UpdatedAt);
    }
}
