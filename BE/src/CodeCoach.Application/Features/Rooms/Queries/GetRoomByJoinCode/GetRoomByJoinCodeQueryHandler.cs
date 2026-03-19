using MediatR;

using CodeCoach.Application.DTOs;
using CodeCoach.Application.Interfaces;

namespace CodeCoach.Application.Features.Rooms.Queries.GetRoomByJoinCode;

public class GetRoomByJoinCodeQueryHandler : IRequestHandler<GetRoomByJoinCodeQuery, RoomDto?>
{
    private readonly IRoomRepository _roomRepository;

    public GetRoomByJoinCodeQueryHandler(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<RoomDto?> Handle(GetRoomByJoinCodeQuery request, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByJoinCodeAsync(request.JoinCode, cancellationToken);

        if (room is null)
        {
            return null;
        }

        return new RoomDto(
            room.Id,
            room.Name,
            room.JoinCode,
            room.MentorId,
            room.Status.ToString());
    }
}
