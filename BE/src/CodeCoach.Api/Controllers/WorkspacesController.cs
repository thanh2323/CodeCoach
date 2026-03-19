using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using CodeCoach.Api.Contracts.Workspaces;
using CodeCoach.Application.Exceptions;
using CodeCoach.Application.Workspaces.Commands.SaveWorkspaceSnapshot;
using CodeCoach.Application.Workspaces.Queries.GetWorkspace;

namespace CodeCoach.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/rooms/{roomId:guid}/workspaces")]
public class WorkspacesController : ControllerBase
{
    private readonly ISender _sender;

    public WorkspacesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetAsync(
        Guid roomId,
        CancellationToken cancellationToken)
    {
        var workspace = await _sender.Send(
            new GetWorkspaceQuery(roomId, GetCurrentUserId()),
            cancellationToken);

        if (workspace is null)
        {
            return NotFound();
        }

        return Ok(workspace);
    }

    [HttpPut("me/snapshot")]
    public async Task<IActionResult> SaveSnapshotAsync(
        Guid roomId,
        [FromBody] SaveWorkspaceSnapshotRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new SaveWorkspaceSnapshotCommand(roomId, GetCurrentUserId(), request.Language, request.SourceCode),
            cancellationToken);

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            throw new UnauthorizedException("Authenticated user id claim is missing.");
        }

        return parsedUserId;
    }
}
