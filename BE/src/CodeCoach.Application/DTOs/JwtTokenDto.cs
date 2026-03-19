namespace CodeCoach.Application.DTOs;

public record JwtTokenDto(string AccessToken, DateTime ExpiresAtUtc);
