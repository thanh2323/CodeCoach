using Microsoft.AspNetCore.Identity;

using CodeCoach.Application.Abstractions;
using CodeCoach.Domain.Entities;

namespace CodeCoach.Infrastructure.Auth;

public class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result != PasswordVerificationResult.Failed;
    }
}
