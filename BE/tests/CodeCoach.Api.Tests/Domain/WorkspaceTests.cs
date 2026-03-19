using System;
using System.Threading;

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

    [Fact]
    public void UpdateSnapshot_UpdatesLanguageSourceCode_AndUpdatedAt()
    {
        var workspace = new Workspace(Guid.NewGuid(), Guid.NewGuid(), "csharp", "before");
        var originalUpdatedAt = workspace.UpdatedAt;

        Thread.Sleep(5);

        workspace.UpdateSnapshot("python", "print('after')");

        Assert.Equal("python", workspace.Language);
        Assert.Equal("print('after')", workspace.SourceCode);
        Assert.True(workspace.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void UpdateSnapshot_Throws_WhenRequiredFieldsMissing()
    {
        var workspace = new Workspace(Guid.NewGuid(), Guid.NewGuid(), "csharp", string.Empty);

        Assert.Throws<ArgumentException>(() => workspace.UpdateSnapshot(" ", "code"));
        Assert.Throws<ArgumentNullException>(() => workspace.UpdateSnapshot("csharp", null!));
    }
}
