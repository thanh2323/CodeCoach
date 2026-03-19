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

        if (user is null)
        {
            throw new NotFoundException("User was not found.");
        }

        return await _transactionManager.ExecuteAsync(async transactionCancellationToken =>
        {
            var joinCode = await GenerateUniqueJoinCodeAsync(transactionCancellationToken);
            var room = new Room(name, joinCode, userId, userId);

            await _roomRepository.AddAsync(room, transactionCancellationToken);

            var participant = new RoomParticipant(room.Id, userId, RoomRole.Mentor);
            await _roomParticipantRepository.AddAsync(participant, transactionCancellationToken);

            var workspace = new Workspace(
                room.Id,
                userId,
                DefaultWorkspaceLanguage,
                DefaultWorkspaceSourceCode);
            await _workspaceRepository.AddAsync(workspace, transactionCancellationToken);

            return new RoomDto(
                room.Id,
                room.Name,
                room.JoinCode,
                room.MentorId,
                room.Status.ToString());
        }, cancellationToken);
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
}
