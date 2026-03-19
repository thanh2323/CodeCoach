using Microsoft.AspNetCore.Http;

namespace CodeCoach.Api.Auth;

public class AuthCookieOptions
{
    public string AccessTokenCookieName { get; set; } = "codecoach_access_token";
    public string RefreshTokenCookieName { get; set; } = "codecoach_refresh_token";
    public SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;
    public bool HttpOnly { get; set; } = true;
    public bool Secure { get; set; } = true;
}
