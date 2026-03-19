using MediatR;

using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Auth.Commands.RegisterUser;

public record RegisterUserCommand(string Name, string Email, string Password) : IRequest<AuthenticatedUserDto>;
