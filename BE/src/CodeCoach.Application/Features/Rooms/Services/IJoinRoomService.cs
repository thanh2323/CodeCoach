using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Features.Rooms.Services;

public interface IJoinRoomService
{
    Task<RoomDetailsDto> JoinAsync(string joinCode, Guid userId, CancellationToken cancellationToken = default);
}
