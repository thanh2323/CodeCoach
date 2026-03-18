using System;

using Xunit;

using CodeCoach.Domain.Entities;

namespace CodeCoach.Api.Tests.Domain;

public class UserTests
{
    [Fact]
    public void User_Creation_SetsIdAndName_OptionalEmailAvatar()
    {
        var user = new User("Alice", null, null);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Alice", user.Name);
        Assert.Null(user.Email);
        Assert.Null(user.AvatarUrl);
        Assert.NotEqual(default, user.CreatedAt);
    }

    [Fact]
    public void User_Creation_Throws_WhenNameInvalid()
    {
        Assert.Throws<ArgumentException>(() => new User("  ", null, null));
    }
}
