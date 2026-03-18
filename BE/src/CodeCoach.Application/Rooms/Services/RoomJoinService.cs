using System.Collections.Generic;

using CodeCoach.Application.DTOs;
using CodeCoach.Application.Interfaces;
using CodeCoach.Domain.Entities;
using CodeCoach.Domain.Enums;

namespace CodeCoach.Application.Rooms.Services;

public class RoomJoinService : IJoinRoomService
{
    private const string DefaultWorkspaceLanguage = "csharp";
    private const string DefaultWorkspaceSourceCode = "";

    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IWorkspaceRepository _workspaceRepository;

    public RoomJoinService(
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

    public async Task<RoomDetailsDto> JoinAsync(
        string joinCode,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var room = await _roomRepository.GetByJoinCodeAsync(joinCode, cancellationToken);

        if (room is null)
        {
            throw new KeyNotFoundException("Room was not found.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("User was not found.");
        }

        room.EnsureActive();

        var existingParticipant = await _roomParticipantRepository.GetParticipantAsync(
            room.Id,
            userId,
            cancellationToken);

        if (existingParticipant is not null)
        {
            throw new InvalidOperationException("User is already a participant in this room.");
        }

        var participant = new RoomParticipant(room.Id, userId, RoomRole.Student);
        participant = await _roomParticipantRepository.AddAsync(participant, cancellationToken);

        var workspace = new Workspace(
            room.Id,
            userId,
            DefaultWorkspaceLanguage,
            DefaultWorkspaceSourceCode);
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
