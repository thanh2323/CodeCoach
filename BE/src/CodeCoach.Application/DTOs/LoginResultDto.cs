namespace CodeCoach.Application.DTOs;

public record LoginResultDto(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    AuthenticatedUserDto User);
