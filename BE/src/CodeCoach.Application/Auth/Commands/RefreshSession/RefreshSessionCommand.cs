using MediatR;

using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Auth.Commands.RefreshSession;

public record RefreshSessionCommand(string RefreshToken) : IRequest<RefreshSessionResultDto>;
