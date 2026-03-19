using MediatR;

namespace CodeCoach.Application.Auth.Commands.Logout;

public record LogoutCommand(string? RefreshToken) : IRequest<Unit>;
