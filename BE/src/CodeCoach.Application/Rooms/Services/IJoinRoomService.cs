using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Rooms.Services;

public interface IJoinRoomService
{
    Task<RoomDetailsDto> JoinAsync(string joinCode, Guid userId, CancellationToken cancellationToken = default);
}
