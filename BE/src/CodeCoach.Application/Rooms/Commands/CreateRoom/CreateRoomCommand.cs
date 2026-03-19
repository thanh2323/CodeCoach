using System;

using MediatR;

using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Rooms.Commands.CreateRoom;

public record CreateRoomCommand(string Name, Guid UserId) : IRequest<RoomDto>;
