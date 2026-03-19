using MediatR;

namespace CodeCoach.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string? RefreshToken) : IRequest<Unit>;
