using MediatR;

using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Features.Rooms.Queries.GetRoomByJoinCode;

public record GetRoomByJoinCodeQuery(string JoinCode) : IRequest<RoomDto?>;
