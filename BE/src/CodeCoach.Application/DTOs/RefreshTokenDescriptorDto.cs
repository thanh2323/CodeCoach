namespace CodeCoach.Application.DTOs;

public record RefreshTokenDescriptorDto(string Token, string TokenHash, DateTime ExpiresAtUtc);
