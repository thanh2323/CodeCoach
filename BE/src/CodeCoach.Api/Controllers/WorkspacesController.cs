using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using CodeCoach.Api.Contracts.Workspaces;
using CodeCoach.Application.Workspaces.Commands.SaveWorkspaceSnapshot;
using CodeCoach.Application.Workspaces.Queries.GetWorkspace;

namespace CodeCoach.Api.Controllers;

[ApiController]
[Route("api/v1/rooms/{roomId:guid}/workspaces")]
public class WorkspacesController : ControllerBase
{
    private readonly ISender _sender;

    public WorkspacesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetAsync(
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var workspace = await _sender.Send(new GetWorkspaceQuery(roomId, userId), cancellationToken);

        if (workspace is null)
        {
            return NotFound();
        }

        return Ok(workspace);
    }

    [HttpPut("{userId:guid}/snapshot")]
    public async Task<IActionResult> SaveSnapshotAsync(
        Guid roomId,
        Guid userId,
        [FromBody] SaveWorkspaceSnapshotRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new SaveWorkspaceSnapshotCommand(roomId, userId, request.Language, request.SourceCode),
            cancellationToken);

        return NoContent();
    }
}
