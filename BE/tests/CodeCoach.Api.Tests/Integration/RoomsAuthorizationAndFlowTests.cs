using System.Net;
using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;
using CodeCoach.Domain.Enums;

namespace CodeCoach.Api.Tests.Integration;

public class RoomsAuthorizationAndFlowTests
{
    [Theory]
    [InlineData("POST", "/api/v1/rooms")]
    [InlineData("GET", "/api/v1/rooms/JOIN01")]
    [InlineData("POST", "/api/v1/rooms/JOIN01/join")]
    public async Task RoomEndpoints_ReturnUnauthorized_WhenRequestIsAnonymous(string method, string path)
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateApiClient();

        using var request = new HttpRequestMessage(new HttpMethod(method), path);

        if (method == HttpMethod.Post.Method)
        {
            request.Content = JsonContent.Create(new { Name = "Algorithms" });
        }

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRoom_UsesAuthenticatedUserAsMentor_AndCreatesMentorParticipantAndWorkspace()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateApiClient();
        await AuthClientTestHelper.RegisterAsync(client, "Mentor", "mentor@example.com");
        var login = await AuthClientTestHelper.LoginAsync(client, "mentor@example.com");

        var response = await client.PostAsJsonAsync("/api/v1/rooms", new
        {
            Name = "Algorithms"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var room = await response.Content.ReadFromJsonAsync<RoomResponse>();
        Assert.NotNull(room);
        Assert.Equal(login.User.Id, room!.MentorId);

        var participant = await factory.ExecuteDbContextAsync(dbContext =>
            dbContext.RoomParticipants.SingleAsync(item =>
                item.RoomId == room.Id &&
                item.UserId == login.User.Id));

        Assert.Equal(RoomRole.Mentor, participant.Role);

        var workspace = await factory.ExecuteDbContextAsync(dbContext =>
            dbContext.Workspaces.SingleAsync(item =>
                item.RoomId == room.Id &&
                item.UserId == login.User.Id));

        Assert.Equal("csharp", workspace.Language);
        Assert.Equal(string.Empty, workspace.SourceCode);
    }

    [Fact]
    public async Task JoinRoom_UsesAuthenticatedUserAsStudent_AndCreatesStudentParticipantAndWorkspace()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var mentorClient = factory.CreateApiClient();
        await AuthClientTestHelper.RegisterAsync(mentorClient, "Mentor", "mentor@example.com");
        await AuthClientTestHelper.LoginAsync(mentorClient, "mentor@example.com");

        var createResponse = await mentorClient.PostAsJsonAsync("/api/v1/rooms", new
        {
            Name = "Algorithms"
        });

        var createdRoom = await createResponse.Content.ReadFromJsonAsync<RoomResponse>();

        using var studentClient = factory.CreateApiClient();
        await AuthClientTestHelper.RegisterAsync(studentClient, "Student", "student@example.com");
        var studentLogin = await AuthClientTestHelper.LoginAsync(studentClient, "student@example.com");

        var joinResponse = await studentClient.PostAsJsonAsync(
            $"/api/v1/rooms/{createdRoom!.JoinCode}/join",
            new { });

        Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

        var roomDetails = await joinResponse.Content.ReadFromJsonAsync<RoomDetailsResponse>();
        Assert.NotNull(roomDetails);
        Assert.Equal(studentLogin.User.Id, roomDetails!.UserId);

        var participant = await factory.ExecuteDbContextAsync(dbContext =>
            dbContext.RoomParticipants.SingleAsync(item =>
                item.RoomId == createdRoom.Id &&
                item.UserId == studentLogin.User.Id));

        Assert.Equal(RoomRole.Student, participant.Role);

        var workspace = await factory.ExecuteDbContextAsync(dbContext =>
            dbContext.Workspaces.SingleAsync(item =>
                item.RoomId == createdRoom.Id &&
                item.UserId == studentLogin.User.Id));

        Assert.Equal(studentLogin.User.Id, workspace.UserId);
    }

    [Fact]
    public async Task ProtectedRoomEndpoints_DoNotAcceptBearerHeaderWithoutCookies()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var loginClient = factory.CreateApiClient();

        await AuthClientTestHelper.RegisterAsync(loginClient, "Mentor", "mentor@example.com");
        var login = await AuthClientTestHelper.LoginAsync(loginClient, "mentor@example.com");

        using var bearerOnlyClient = factory.CreateApiClient();
        bearerOnlyClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login.AccessToken);

        var response = await bearerOnlyClient.PostAsJsonAsync("/api/v1/rooms", new
        {
            Name = "Algorithms"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed record RoomResponse(Guid Id, string Name, string JoinCode, Guid MentorId, string Status);

    private sealed record RoomDetailsResponse(
        Guid Id,
        string Name,
        string JoinCode,
        Guid MentorId,
        string Status,
        string CurrentMode,
        Guid ParticipantId,
        Guid UserId,
        Guid WorkspaceId,
        string Language,
        string SourceCode);

}
