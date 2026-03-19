using System;

using MediatR;

using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Features.Rooms.Commands.CreateRoom;

public record CreateRoomCommand(string Name, Guid UserId) : IRequest<RoomDto>;
