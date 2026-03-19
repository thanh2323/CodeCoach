using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Features.Rooms.Services;

public interface ICreateRoomService
{
    Task<RoomDto> CreateAsync(string name, Guid userId, CancellationToken cancellationToken = default);
}
