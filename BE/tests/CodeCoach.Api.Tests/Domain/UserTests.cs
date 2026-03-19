using System;

using Xunit;

using CodeCoach.Domain.Entities;

namespace CodeCoach.Api.Tests.Domain;

public class UserTests
{
    [Fact]
    public void User_Creation_SetsIdAndNormalizesEmail()
    {
        var user = new User("Alice", "ALICE@example.com", "hashed-password");

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Alice", user.Name);
        Assert.Equal("alice@example.com", user.Email);
        Assert.Equal("hashed-password", user.PasswordHash);
        Assert.Null(user.AvatarUrl);
        Assert.NotEqual(default, user.CreatedAt);
    }

    [Fact]
    public void User_Creation_Throws_WhenNameInvalid()
    {
        Assert.Throws<ArgumentException>(() => new User("  ", "alice@example.com", "hashed-password"));
    }

    [Fact]
    public void User_Creation_Throws_WhenEmailOrPasswordHashInvalid()
    {
        Assert.Throws<ArgumentException>(() => new User("Alice", "  ", "hashed-password"));
        Assert.Throws<ArgumentException>(() => new User("Alice", "alice@example.com", "  "));
    }
}
