using MediatR;

using CodeCoach.Application.DTOs;
using CodeCoach.Application.Rooms.Services;

namespace CodeCoach.Application.Rooms.Commands.JoinRoom;

public class JoinRoomCommandHandler : IRequestHandler<JoinRoomCommand, RoomDetailsDto>
{
    private readonly IJoinRoomService _joinRoomService;

    public JoinRoomCommandHandler(IJoinRoomService joinRoomService)
    {
        _joinRoomService = joinRoomService;
    }

    public async Task<RoomDetailsDto> Handle(JoinRoomCommand request, CancellationToken cancellationToken)
    {
        return await _joinRoomService.JoinAsync(request.JoinCode, request.UserId, cancellationToken);
    }
}
