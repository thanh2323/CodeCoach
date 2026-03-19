using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using CodeCoach.Domain.Entities;

namespace CodeCoach.Api.Tests.Integration;

public class AuthenticationEndpointsTests
{
    [Fact]
    public async Task Register_ReturnsCreated_NormalizesEmail_AndStoresHashedPassword()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateApiClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Name = "Alice",
            Email = "ALICE@Example.COM",
            Password = "Passw0rd!"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdUser = await response.Content.ReadFromJsonAsync<AuthClientTestHelper.AuthenticatedUserResponse>();
        Assert.NotNull(createdUser);
        Assert.Equal("alice@example.com", createdUser!.Email);
        Assert.False(response.Headers.TryGetValues("Set-Cookie", out _));

        var persistedUser = await factory.ExecuteDbContextAsync(
            dbContext => dbContext.Users.SingleAsync(user => user.Id == createdUser.Id));

        Assert.Equal("alice@example.com", persistedUser.Email);

        var passwordHashProperty = typeof(User).GetProperty(nameof(User.Name).Replace("Name", "PasswordHash"));
        Assert.NotNull(passwordHashProperty);
        Assert.NotEqual("Passw0rd!", passwordHashProperty!.GetValue(persistedUser) as string);
    }

    [Fact]
    public async Task Register_ReturnsConflict_WhenEmailAlreadyExists()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateApiClient();

        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Name = "Alice",
            Email = "alice@example.com",
            Password = "Passw0rd!"
        });

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Name = "Alice Clone",
            Email = "ALICE@example.com",
            Password = "Passw0rd!"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsOk_SetsAccessAndRefreshCookies_AndDoesNotExposeTokensInBody()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateApiClient();

        await AuthClientTestHelper.RegisterAsync(client, "Alice", "alice@example.com");

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = "alice@example.com",
            Password = "Passw0rd!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<AuthClientTestHelper.AuthenticatedUserResponse>(
            responseBody,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(user);
        Assert.Equal("alice@example.com", user!.Email);

        AuthClientTestHelper.AssertSecureHttpOnlyLaxCookie(response, AuthClientTestHelper.AccessTokenCookieName);
        AuthClientTestHelper.AssertSecureHttpOnlyLaxCookie(response, AuthClientTestHelper.RefreshTokenCookieName);
        Assert.False(string.IsNullOrWhiteSpace(
            AuthClientTestHelper.GetCookieValue(response, AuthClientTestHelper.AccessTokenCookieName)));
        Assert.False(string.IsNullOrWhiteSpace(
            AuthClientTestHelper.GetCookieValue(response, AuthClientTestHelper.RefreshTokenCookieName)));

        using var jsonDocument = JsonDocument.Parse(responseBody);
        Assert.False(jsonDocument.RootElement.TryGetProperty("accessToken", out _));
        Assert.False(jsonDocument.RootElement.TryGetProperty("tokenType", out _));
        Assert.False(jsonDocument.RootElement.TryGetProperty("expiresAtUtc", out _));

        var refreshTokens = await factory.ExecuteDbContextAsync(
            dbContext => Task.FromResult(GetRefreshTokenEntities(dbContext)));
        Assert.Single(refreshTokens);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateApiClient();

        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Name = "Alice",
            Email = "alice@example.com",
            Password = "Passw0rd!"
        });

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = "alice@example.com",
            Password = "wrong-password"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ReturnsNoContent_RotatesRefreshToken_AndSetsNewCookies()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateApiClient();

        await AuthClientTestHelper.RegisterAsync(client, "Alice", "alice@example.com");
        var login = await AuthClientTestHelper.LoginAsync(client, "alice@example.com");

        var response = await client.PostAsync("/api/v1/auth/refresh", content: null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        AuthClientTestHelper.AssertSecureHttpOnlyLaxCookie(response, AuthClientTestHelper.AccessTokenCookieName);
        AuthClientTestHelper.AssertSecureHttpOnlyLaxCookie(response, AuthClientTestHelper.RefreshTokenCookieName);

        var rotatedRefreshToken = AuthClientTestHelper.GetCookieValue(
            response,
            AuthClientTestHelper.RefreshTokenCookieName);
        Assert.NotEqual(login.RefreshToken, rotatedRefreshToken);

        var refreshTokens = await factory.ExecuteDbContextAsync(
            dbContext => Task.FromResult(GetRefreshTokenEntities(dbContext)));
        Assert.Equal(2, refreshTokens.Count);

        var revokedCount = refreshTokens.Count(entity => GetPropertyValue<DateTime?>(entity, "RevokedAtUtc") is not null);
        var activeCount = refreshTokens.Count(entity => GetPropertyValue<DateTime?>(entity, "RevokedAtUtc") is null);

        Assert.Equal(1, revokedCount);
        Assert.Equal(1, activeCount);
        Assert.Contains(refreshTokens, entity => GetPropertyValue<Guid?>(entity, "ReplacedByTokenId") is not null);
    }

    [Fact]
    public async Task Refresh_ReturnsUnauthorized_WhenRefreshCookieIsMissing()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateApiClient();

        var response = await client.PostAsync("/api/v1/auth/refresh", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ReturnsUnauthorized_WhenRotatedRefreshTokenIsReused()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateApiClient();

        await AuthClientTestHelper.RegisterAsync(client, "Alice", "alice@example.com");
        var login = await AuthClientTestHelper.LoginAsync(client, "alice@example.com");

        var refreshResponse = await client.PostAsync("/api/v1/auth/refresh", content: null);
        Assert.Equal(HttpStatusCode.NoContent, refreshResponse.StatusCode);

        using var reuseClient = factory.CreateApiClient();
        using var reuseRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
        reuseRequest.Headers.Add(
            "Cookie",
            AuthClientTestHelper.CreateCookieHeader(
                (AuthClientTestHelper.RefreshTokenCookieName, login.RefreshToken)));

        var reuseResponse = await reuseClient.SendAsync(reuseRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_ReturnsNoContent_RevokesCurrentSession_AndClearsCookies()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateApiClient();

        await AuthClientTestHelper.RegisterAsync(client, "Alice", "alice@example.com");
        await AuthClientTestHelper.LoginAsync(client, "alice@example.com");

        var response = await client.PostAsync("/api/v1/auth/logout", content: null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        AuthClientTestHelper.AssertCookieCleared(response, AuthClientTestHelper.AccessTokenCookieName);
        AuthClientTestHelper.AssertCookieCleared(response, AuthClientTestHelper.RefreshTokenCookieName);

        var refreshTokens = await factory.ExecuteDbContextAsync(
            dbContext => Task.FromResult(GetRefreshTokenEntities(dbContext)));
        Assert.Single(refreshTokens);
        Assert.NotNull(GetPropertyValue<DateTime?>(refreshTokens.Single(), "RevokedAtUtc"));
    }

    private static List<object> GetRefreshTokenEntities(DbContext dbContext)
    {
        var property = dbContext.GetType().GetProperty("RefreshTokens");
        Assert.NotNull(property);

        var value = property!.GetValue(dbContext);
        Assert.NotNull(value);

        return ((System.Collections.IEnumerable)value!)
            .Cast<object>()
            .ToList();
    }

    private static TProperty? GetPropertyValue<TProperty>(object entity, string propertyName)
    {
        var property = entity.GetType().GetProperty(propertyName);
        Assert.NotNull(property);
        return (TProperty?)property!.GetValue(entity);
    }
}
