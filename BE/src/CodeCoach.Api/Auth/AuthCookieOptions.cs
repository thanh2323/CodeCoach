using Microsoft.AspNetCore.Http;

namespace CodeCoach.Api.Auth;

public class AuthCookieOptions
{
    public const string DefaultAccessTokenCookieName = "codecoach_access_token";
    public const string DefaultRefreshTokenCookieName = "codecoach_refresh_token";

    public string AccessTokenCookieName { get; set; } = DefaultAccessTokenCookieName;
    public string RefreshTokenCookieName { get; set; } = DefaultRefreshTokenCookieName;
    public SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;
    public bool HttpOnly { get; set; } = true;
    public bool Secure { get; set; } = true;
}
