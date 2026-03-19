using MediatR;

using CodeCoach.Application.Abstractions;
using CodeCoach.Application.DTOs;
using CodeCoach.Application.Exceptions;
using CodeCoach.Application.Interfaces;
using CodeCoach.Domain.Entities;

namespace CodeCoach.Application.Auth.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthenticatedUserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthenticatedUserDto> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            throw new ConflictException("Email is already registered.");
        }

        var passwordHash = _passwordHasher.HashPassword(
            new User(request.Name, normalizedEmail, "pending"),
            request.Password);

        var user = new User(request.Name, normalizedEmail, passwordHash);
        await _userRepository.AddAsync(user, cancellationToken);

        return new AuthenticatedUserDto(user.Id, user.Name, user.Email);
    }
}
