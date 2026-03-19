using System.Collections.Generic;

using CodeCoach.Application.Abstractions;
using CodeCoach.Application.DTOs;
using CodeCoach.Application.Exceptions;
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
    private readonly ITransactionManager _transactionManager;

    public RoomJoinService(
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IWorkspaceRepository workspaceRepository,
        ITransactionManager transactionManager)
    {
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _workspaceRepository = workspaceRepository;
        _transactionManager = transactionManager;
    }

    public async Task<RoomDetailsDto> JoinAsync(
        string joinCode,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var room = await _roomRepository.GetByJoinCodeAsync(joinCode, cancellationToken);

        if (room is null)
        {
            throw new NotFoundException("Room was not found.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User was not found.");
        }

        try
        {
            room.EnsureActive();
        }
        catch (InvalidOperationException exception)
        {
            throw new ConflictException(exception.Message);
        }

        return await _transactionManager.ExecuteAsync(async transactionCancellationToken =>
        {
            var existingParticipant = await _roomParticipantRepository.GetParticipantAsync(
                room.Id,
                userId,
                transactionCancellationToken);

            if (existingParticipant is not null)
            {
                throw new ConflictException("User is already a participant in this room.");
            }

            var participant = new RoomParticipant(room.Id, userId, RoomRole.Student);
            participant = await _roomParticipantRepository.AddAsync(participant, transactionCancellationToken);

            var workspace = new Workspace(
                room.Id,
                userId,
                DefaultWorkspaceLanguage,
                DefaultWorkspaceSourceCode);
            workspace = await _workspaceRepository.AddAsync(workspace, transactionCancellationToken);

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
        }, cancellationToken);
    }
}
