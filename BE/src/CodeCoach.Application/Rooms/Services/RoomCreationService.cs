using CodeCoach.Application.Abstractions;
using CodeCoach.Application.DTOs;
using CodeCoach.Application.Exceptions;
using CodeCoach.Application.Interfaces;
using CodeCoach.Domain.Entities;
using CodeCoach.Domain.Enums;

namespace CodeCoach.Application.Rooms.Services;

public class RoomCreationService : ICreateRoomService
{
    private const string DefaultWorkspaceLanguage = "csharp";
    private const string DefaultWorkspaceSourceCode = "";

    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ITransactionManager _transactionManager;

    public RoomCreationService(
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

    public async Task<RoomDto> CreateAsync(
        string name,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        ValidateUserExists(user);

        return await _transactionManager.ExecuteAsync(async ct =>
        {
            var room = await CreateRoomWithJoinCodeAsync(name, userId, ct);
            await AddMentorParticipantAsync(room, userId, ct);
            await CreateDefaultWorkspaceAsync(room.Id, userId, ct);
            return MapToDto(room);
        }, cancellationToken);
    }

    private void ValidateUserExists(User? user)
    {
        if (user is null)
        {
            throw new NotFoundException("User was not found.");
        }
    }

    private async Task<Room> CreateRoomWithJoinCodeAsync(
        string name,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var joinCode = await GenerateUniqueJoinCodeAsync(cancellationToken);
        var room = new Room(name, joinCode, userId, userId);
        return await _roomRepository.AddAsync(room, cancellationToken);
    }

    private async Task<string> GenerateUniqueJoinCodeAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var candidate = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            var existingRoom = await _roomRepository.GetByJoinCodeAsync(candidate, cancellationToken);

            if (existingRoom is null)
            {
                return candidate;
            }
        }
    }

    private async Task AddMentorParticipantAsync(
        Room room,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var participant = new RoomParticipant(room.Id, userId, RoomRole.Mentor);
        await _roomParticipantRepository.AddAsync(participant, cancellationToken);
    }

    private async Task CreateDefaultWorkspaceAsync(
        Guid roomId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var workspace = new Workspace(roomId, userId, DefaultWorkspaceLanguage, DefaultWorkspaceSourceCode);
        await _workspaceRepository.AddAsync(workspace, cancellationToken);
    }

    private RoomDto MapToDto(Room room)
    {
        return new RoomDto(
            room.Id,
            room.Name,
            room.JoinCode,
            room.MentorId,
            room.Status.ToString());
    }
}
