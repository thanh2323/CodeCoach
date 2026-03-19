using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using CodeCoach.Api.Auth;
using CodeCoach.Api.Contracts.Auth;
using CodeCoach.Application.Auth.Commands.Logout;
using CodeCoach.Application.Auth.Commands.LoginUser;
using CodeCoach.Application.Auth.Commands.RefreshSession;
using CodeCoach.Application.Auth.Commands.RegisterUser;
using CodeCoach.Application.Exceptions;

namespace CodeCoach.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly AuthCookieOptions _authCookieOptions;

    public AuthController(ISender sender, IOptions<AuthCookieOptions> authCookieOptions)
    {
        _sender = sender;
        _authCookieOptions = authCookieOptions.Value;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _sender.Send(
            new RegisterUserCommand(request.Name, request.Email, request.Password),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            new AuthenticatedUserResponse(user.Id, user.Name, user.Email));
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new LoginUserCommand(request.Email, request.Password),
            cancellationToken);

        AppendAuthCookies(
            result.AccessToken,
            result.AccessTokenExpiresAtUtc,
            result.RefreshToken,
            result.RefreshTokenExpiresAtUtc);

        return Ok(new AuthenticatedUserResponse(result.User.Id, result.User.Name, result.User.Email));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync(CancellationToken cancellationToken)
    {
        var refreshToken = GetRefreshTokenFromCookie();
        var result = await _sender.Send(new RefreshSessionCommand(refreshToken), cancellationToken);

        AppendAuthCookies(
            result.AccessToken,
            result.AccessTokenExpiresAtUtc,
            result.RefreshToken,
            result.RefreshTokenExpiresAtUtc);

        return NoContent();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
    {
        Request.Cookies.TryGetValue(_authCookieOptions.RefreshTokenCookieName, out var refreshToken);

        await _sender.Send(new LogoutCommand(refreshToken), cancellationToken);
        DeleteAuthCookies();

        return NoContent();
    }

    private string GetRefreshTokenFromCookie()
    {
        if (!Request.Cookies.TryGetValue(_authCookieOptions.RefreshTokenCookieName, out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedException("Refresh token is required.");
        }

        return refreshToken;
    }

    private void AppendAuthCookies(
        string accessToken,
        DateTime accessTokenExpiresAtUtc,
        string refreshToken,
        DateTime refreshTokenExpiresAtUtc)
    {
        Response.Cookies.Append(
            _authCookieOptions.AccessTokenCookieName,
            accessToken,
            BuildCookieOptions(accessTokenExpiresAtUtc));

        Response.Cookies.Append(
            _authCookieOptions.RefreshTokenCookieName,
            refreshToken,
            BuildCookieOptions(refreshTokenExpiresAtUtc));
    }

    private void DeleteAuthCookies()
    {
        Response.Cookies.Delete(_authCookieOptions.AccessTokenCookieName, BuildCookieOptions(DateTime.UtcNow.AddDays(-1)));
        Response.Cookies.Delete(_authCookieOptions.RefreshTokenCookieName, BuildCookieOptions(DateTime.UtcNow.AddDays(-1)));
    }

    private CookieOptions BuildCookieOptions(DateTime expiresAtUtc)
    {
        return new CookieOptions
        {
            HttpOnly = _authCookieOptions.HttpOnly,
            Secure = _authCookieOptions.Secure,
            SameSite = _authCookieOptions.SameSite,
            Expires = expiresAtUtc,
            Path = "/"
        };
    }
}
