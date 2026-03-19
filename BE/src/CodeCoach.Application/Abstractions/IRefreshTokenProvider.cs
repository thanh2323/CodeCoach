using CodeCoach.Application.DTOs;

namespace CodeCoach.Application.Abstractions;

public interface IRefreshTokenProvider
{
    RefreshTokenDescriptorDto Generate();
    string ComputeHash(string token);
}
