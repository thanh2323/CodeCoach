using System;

using Xunit;

using CodeCoach.Domain.Entities;

namespace CodeCoach.Api.Tests.Domain;

public class WorkspaceTests
{
    [Fact]
    public void Workspace_AllowsNullableUserId_AndSetsUpdatedAt()
    {
        var workspace = new Workspace(Guid.NewGuid(), null, "csharp", string.Empty);

        Assert.Null(workspace.UserId);
        Assert.Equal("csharp", workspace.Language);
        Assert.NotEqual(default, workspace.UpdatedAt);
    }

    [Fact]
    public void Workspace_Creation_Throws_WhenRequiredFieldsMissing()
    {
        Assert.Throws<ArgumentException>(() => new Workspace(Guid.Empty, Guid.NewGuid(), "csharp", string.Empty));
        Assert.Throws<ArgumentException>(() => new Workspace(Guid.NewGuid(), Guid.NewGuid(), " ", string.Empty));
        Assert.Throws<ArgumentNullException>(() => new Workspace(Guid.NewGuid(), Guid.NewGuid(), "csharp", null!));
    }
}
