using MediatR;

using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Rooms.Queries.GetRoomByJoinCode;

public record GetRoomByJoinCodeQuery(string JoinCode) : IRequest<RoomDto?>;
