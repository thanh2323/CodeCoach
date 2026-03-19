using System;

using MediatR;

using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Features.Rooms.Commands.JoinRoom;

public record JoinRoomCommand(string JoinCode, Guid UserId) : IRequest<RoomDetailsDto>;
