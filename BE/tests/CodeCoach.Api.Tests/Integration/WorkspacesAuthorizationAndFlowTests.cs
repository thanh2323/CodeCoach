using System.Net;
using System.Net.Http.Json;

namespace CodeCoach.Api.Tests.Integration;

public class WorkspacesAuthorizationAndFlowTests
{
    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    public async Task WorkspaceCurrentUserEndpoints_ReturnUnauthorized_WhenRequestIsAnonymous(string method)
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateApiClient();
        var roomId = Guid.NewGuid();

        using var request = new HttpRequestMessage(
            new HttpMethod(method),
            $"/api/v1/rooms/{roomId}/workspaces/me");

        if (method == HttpMethod.Put.Method)
        {
            request.RequestUri = new Uri($"/api/v1/rooms/{roomId}/workspaces/me/snapshot", UriKind.Relative);
            request.Content = JsonContent.Create(new
            {
                Language = "python",
                SourceCode = "print('hello')"
            });
        }

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WorkspaceCurrentUserEndpoints_ReadAndWriteCurrentUsersWorkspace()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateApiClient();
        await AuthClientTestHelper.RegisterAsync(client, "Mentor", "mentor@example.com");
        var login = await AuthClientTestHelper.LoginAsync(client, "mentor@example.com");

        var createRoomResponse = await client.PostAsJsonAsync("/api/v1/rooms", new
        {
            Name = "Algorithms"
        });

        var room = await createRoomResponse.Content.ReadFromJsonAsync<RoomResponse>();

        var getResponse = await client.GetAsync($"/api/v1/rooms/{room!.Id}/workspaces/me");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var workspace = await getResponse.Content.ReadFromJsonAsync<WorkspaceResponse>();
        Assert.NotNull(workspace);
        Assert.Equal(login.User.Id, workspace!.UserId);

        var saveResponse = await client.PutAsJsonAsync(
            $"/api/v1/rooms/{room.Id}/workspaces/me/snapshot",
            new
            {
                Language = "python",
                SourceCode = "print('hello')"
            });

        Assert.Equal(HttpStatusCode.NoContent, saveResponse.StatusCode);

        var updatedWorkspace = await factory.ExecuteDbContextAsync(dbContext =>
            Task.FromResult(dbContext.Workspaces.Single(item =>
                item.RoomId == room.Id &&
                item.UserId == login.User.Id)));

        Assert.Equal("python", updatedWorkspace.Language);
        Assert.Equal("print('hello')", updatedWorkspace.SourceCode);
    }

    private sealed record RoomResponse(Guid Id, string Name, string JoinCode, Guid MentorId, string Status);

    private sealed record WorkspaceResponse(
        Guid Id,
        Guid RoomId,
        Guid? UserId,
        string Language,
        string SourceCode,
        DateTime UpdatedAt);

}
