using System.Net;
using System.Net.Http.Json;

namespace CodeCoach.Api.Tests.Integration;

internal static class AuthClientTestHelper
{
    internal const string AccessTokenCookieName = "codecoach_access_token";
    internal const string RefreshTokenCookieName = "codecoach_refresh_token";

    internal static async Task<AuthenticatedUserResponse> RegisterAsync(
        HttpClient client,
        string name,
        string email,
        string password = "Passw0rd!")
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Name = name,
            Email = email,
            Password = password
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<AuthenticatedUserResponse>();
        Assert.NotNull(user);
        return user!;
    }

    internal static async Task<LoginSession> LoginAsync(
        HttpClient client,
        string email,
        string password = "Passw0rd!")
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = email,
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<AuthenticatedUserResponse>();
        Assert.NotNull(user);

        var accessCookie = GetCookieValue(response, AccessTokenCookieName);
        var refreshCookie = GetCookieValue(response, RefreshTokenCookieName);

        return new LoginSession(user!, accessCookie, refreshCookie, response);
    }

    internal static string GetSetCookieHeader(HttpResponseMessage response, string cookieName)
    {
        return response.Headers
            .GetValues("Set-Cookie")
            .Single(header => header.StartsWith($"{cookieName}=", StringComparison.Ordinal));
    }

    internal static string GetCookieValue(HttpResponseMessage response, string cookieName)
    {
        var header = GetSetCookieHeader(response, cookieName);
        var cookieSegment = header.Split(';', 2)[0];
        return cookieSegment.Split('=', 2)[1];
    }

    internal static void AssertSecureHttpOnlyLaxCookie(HttpResponseMessage response, string cookieName)
    {
        var header = GetSetCookieHeader(response, cookieName);

        Assert.Contains($"{cookieName}=", header, StringComparison.Ordinal);
        Assert.Contains("httponly", header, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", header, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", header, StringComparison.OrdinalIgnoreCase);
    }

    internal static void AssertCookieCleared(HttpResponseMessage response, string cookieName)
    {
        var header = GetSetCookieHeader(response, cookieName);

        Assert.Contains($"{cookieName}=", header, StringComparison.Ordinal);
        Assert.True(
            header.Contains("expires=", StringComparison.OrdinalIgnoreCase) ||
            header.Contains("max-age=0", StringComparison.OrdinalIgnoreCase),
            $"Expected cookie '{cookieName}' to be cleared, but header was '{header}'.");
    }

    internal static string CreateCookieHeader(params (string Name, string Value)[] cookies)
    {
        return string.Join("; ", cookies.Select(cookie => $"{cookie.Name}={cookie.Value}"));
    }

    internal sealed record AuthenticatedUserResponse(Guid Id, string Name, string Email);

    internal sealed record LoginSession(
        AuthenticatedUserResponse User,
        string AccessToken,
        string RefreshToken,
        HttpResponseMessage Response);
}
