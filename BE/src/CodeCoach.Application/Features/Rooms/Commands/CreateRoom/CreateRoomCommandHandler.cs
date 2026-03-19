using MediatR;

using CodeCoach.Application.DTOs;
using CodeCoach.Application.Features.Rooms.Services;

namespace CodeCoach.Application.Features.Rooms.Commands.CreateRoom;

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, RoomDto>
{
    private readonly ICreateRoomService _createRoomService;

    public CreateRoomCommandHandler(ICreateRoomService createRoomService)
    {
        _createRoomService = createRoomService;
    }

    public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        return await _createRoomService.CreateAsync(request.Name, request.UserId, cancellationToken);
    }
}
