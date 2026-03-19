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
        ValidateRoomExists(room);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        ValidateUserExists(user);

        ValidateRoomActive(room!);

        return await _transactionManager.ExecuteAsync(async ct =>
        {
            await EnsureNotAlreadyParticipantAsync(room!.Id, user!.Id, ct);
            var participant = await AddParticipantAsync(room.Id, user.Id, ct);
            var workspace = await CreateWorkspaceAsync(room.Id, user.Id, ct);
            return MapToDto(room, participant, workspace);
        }, cancellationToken);
    }

    private void ValidateRoomExists(Room? room)
    {
        if (room is null)
        {
            throw new NotFoundException("Room was not found.");
        }
    }

    private void ValidateUserExists(User? user)
    {
        if (user is null)
        {
            throw new NotFoundException("User was not found.");
        }
    }

    private void ValidateRoomActive(Room room)
    {
        try
        {
            room.EnsureActive();
        }
        catch (InvalidOperationException exception)
        {
            throw new ConflictException(exception.Message);
        }
    }

    private async Task EnsureNotAlreadyParticipantAsync(
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var existing = await _roomParticipantRepository.GetParticipantAsync(roomId, userId, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("User is already a participant in this room.");
        }
    }

    private async Task<RoomParticipant> AddParticipantAsync(
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var participant = new RoomParticipant(roomId, userId, RoomRole.Student);
        return await _roomParticipantRepository.AddAsync(participant, cancellationToken);
    }

    private async Task<Workspace> CreateWorkspaceAsync(
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var workspace = new Workspace(roomId, userId, DefaultWorkspaceLanguage, DefaultWorkspaceSourceCode);
        return await _workspaceRepository.AddAsync(workspace, cancellationToken);
    }

    private RoomDetailsDto MapToDto(Room room, RoomParticipant participant, Workspace workspace)
    {
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
