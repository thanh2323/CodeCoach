using System.Collections.Generic;

using MediatR;

using CodeCoach.Application.Exceptions;
using CodeCoach.Application.Interfaces;

namespace CodeCoach.Application.Features.Workspaces.Commands.SaveWorkspaceSnapshot;

public class SaveWorkspaceSnapshotCommandHandler : IRequestHandler<SaveWorkspaceSnapshotCommand, Unit>
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public SaveWorkspaceSnapshotCommandHandler(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public async Task<Unit> Handle(SaveWorkspaceSnapshotCommand request, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetWorkspaceAsync(
            request.RoomId,
            request.UserId,
            cancellationToken);

        if (workspace is null)
        {
            throw new NotFoundException("Workspace was not found.");
        }

        workspace.UpdateSnapshot(request.Language, request.SourceCode);

        await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

        return Unit.Value;
    }
}
