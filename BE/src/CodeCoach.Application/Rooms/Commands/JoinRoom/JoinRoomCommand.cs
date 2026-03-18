using System;

using MediatR;

using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Rooms.Commands.JoinRoom;

public record JoinRoomCommand(string JoinCode, Guid UserId) : IRequest<RoomDetailsDto>;
