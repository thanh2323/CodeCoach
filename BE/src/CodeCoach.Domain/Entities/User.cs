using System;

namespace CodeCoach.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User()
    {
        Name = string.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
    }

    public User(string name, string email, string passwordHash, string? avatarUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("PasswordHash is required.", nameof(passwordHash));
        }

        Id = Guid.NewGuid();
        Name = name;
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        AvatarUrl = avatarUrl;
        CreatedAt = DateTime.UtcNow;
    }
}
