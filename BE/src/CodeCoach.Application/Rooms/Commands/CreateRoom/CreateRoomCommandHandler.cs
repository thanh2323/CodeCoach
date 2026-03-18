using MediatR;

using CodeCoach.Application.Abstractions;
using CodeCoach.Application.DTOs;
using CodeCoach.Domain.Entities;
using CodeCoach.Application.Interfaces;

namespace CodeCoach.Application.Rooms.Commands.CreateRoom;

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, RoomDto>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IJoinCodeGenerator _joinCodeGenerator;

    public CreateRoomCommandHandler(
        IRoomRepository roomRepository,
        IJoinCodeGenerator joinCodeGenerator)
    {
        _roomRepository = roomRepository;
        _joinCodeGenerator = joinCodeGenerator;
    }

    public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var joinCode = await GenerateUniqueJoinCodeAsync(cancellationToken);
        var room = new Room(request.Name, joinCode, request.MentorId, request.MentorId);

        await _roomRepository.AddAsync(room, cancellationToken);

        return new RoomDto(
            room.Id,
            room.Name,
            room.JoinCode,
            room.MentorId,
            room.Status.ToString());
    }

    private async Task<string> GenerateUniqueJoinCodeAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var candidate = _joinCodeGenerator.Generate();
            var existingRoom = await _roomRepository.GetByJoinCodeAsync(candidate, cancellationToken);

            if (existingRoom is null)
            {
                return candidate;
            }
        }
    }
}
