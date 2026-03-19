using MediatR;

using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Auth.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<LoginResultDto>;
