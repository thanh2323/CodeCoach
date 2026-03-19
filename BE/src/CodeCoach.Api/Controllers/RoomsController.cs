using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using CodeCoach.Api.Contracts.Rooms;
using CodeCoach.Application.Exceptions;
using CodeCoach.Application.Features.Rooms.Commands.JoinRoom;
using CodeCoach.Application.Features.Rooms.Commands.CreateRoom;
using CodeCoach.Application.Features.Rooms.Queries.GetRoomByJoinCode;

namespace CodeCoach.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/rooms")]
public class RoomsController : ControllerBase
{
    private readonly ISender _sender;

    public RoomsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateRoomRequest request,
        CancellationToken cancellationToken)
    {
        var room = await _sender.Send(
            new CreateRoomCommand(request.Name, GetCurrentUserId()),
            cancellationToken);

        return Created($"/api/v1/rooms/{room.JoinCode}", room);
    }

    [HttpGet("{joinCode}")]
    public async Task<IActionResult> GetByJoinCodeAsync(
        string joinCode,
        CancellationToken cancellationToken)
    {
        var room = await _sender.Send(new GetRoomByJoinCodeQuery(joinCode), cancellationToken);

        if (room is null)
        {
            return NotFound();
        }

        return Ok(room);
    }

    [HttpPost("{joinCode}/join")]
    public async Task<IActionResult> JoinAsync(
        string joinCode,
        CancellationToken cancellationToken)
    {
        var room = await _sender.Send(new JoinRoomCommand(joinCode, GetCurrentUserId()), cancellationToken);

        return Ok(room);
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
