using System;

namespace CodeCoach.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Email { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User(string name, string? email, string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        AvatarUrl = avatarUrl;
        CreatedAt = DateTime.UtcNow;
    }
}
