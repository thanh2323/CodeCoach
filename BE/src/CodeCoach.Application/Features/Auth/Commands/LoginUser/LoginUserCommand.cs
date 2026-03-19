using MediatR;

using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Features.Auth.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<LoginResultDto>;
