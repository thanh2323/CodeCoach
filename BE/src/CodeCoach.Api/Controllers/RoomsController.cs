using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using CodeCoach.Api.Contracts.Rooms;
using CodeCoach.Application.Rooms.Commands.CreateRoom;
using CodeCoach.Application.Rooms.Queries.GetRoomByJoinCode;

namespace CodeCoach.Api.Controllers;

[ApiController]
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
            new CreateRoomCommand(request.Name, request.MentorId),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetByJoinCodeAsync),
            new { joinCode = room.JoinCode },
            room);
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
}
