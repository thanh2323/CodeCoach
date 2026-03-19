using MediatR;

using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Features.Auth.Commands.RefreshSession;

public record RefreshSessionCommand(string RefreshToken) : IRequest<RefreshSessionResultDto>;
