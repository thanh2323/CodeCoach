using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Rooms.Services;

public interface ICreateRoomService
{
    Task<RoomDto> CreateAsync(string name, Guid userId, CancellationToken cancellationToken = default);
}
