using System;
using System.Collections.Generic;

using MediatR;

using CodeCoach.Application.DTOs;
using CodeCoach.Application.Interfaces;
using CodeCoach.Domain.Entities;
using CodeCoach.Domain.Enums;

namespace CodeCoach.Application.Rooms.Commands.JoinRoom;

public class JoinRoomCommandHandler : IRequestHandler<JoinRoomCommand, RoomDetailsDto>
{
    private const string DefaultWorkspaceLanguage = "csharp";
    private const string DefaultWorkspaceSourceCode = "";

    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IWorkspaceRepository _workspaceRepository;

    public JoinRoomCommandHandler(
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IWorkspaceRepository workspaceRepository)
    {
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _workspaceRepository = workspaceRepository;
    }

    public async Task<RoomDetailsDto> Handle(JoinRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetByJoinCodeAsync(request.JoinCode, cancellationToken);

        if (room is null)
        {
            throw new KeyNotFoundException("Room was not found.");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("User was not found.");
        }

        if (room.Status != RoomStatus.Active)
        {
            throw new InvalidOperationException("Room is not active.");
        }

        var existingParticipant = await _roomParticipantRepository.GetParticipantAsync(
            room.Id,
            request.UserId,
            cancellationToken);

        if (existingParticipant is not null)
        {
            throw new InvalidOperationException("User is already a participant in this room.");
        }

        var participant = new RoomParticipant(room.Id, request.UserId, RoomRole.Student);
        var workspace = new Workspace(
            room.Id,
            request.UserId,
            DefaultWorkspaceLanguage,
            DefaultWorkspaceSourceCode);

        participant = await _roomParticipantRepository.AddAsync(participant, cancellationToken);
        workspace = await _workspaceRepository.AddAsync(workspace, cancellationToken);

        return new RoomDetailsDto(
            room.Id,
            room.Name,
            room.JoinCode,
            room.MentorId,
            room.Status.ToString(),
            room.CurrentMode.ToString(),
            participant.Id,
            participant.UserId,
            workspace.Id,
            workspace.Language,
            workspace.SourceCode);
    }
}
