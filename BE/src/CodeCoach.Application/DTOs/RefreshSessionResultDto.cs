namespace CodeCoach.Application.DTOs;

public record RefreshSessionResultDto(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
