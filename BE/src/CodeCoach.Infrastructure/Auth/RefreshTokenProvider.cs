using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;

using CodeCoach.Application.Abstractions;
using CodeCoach.Application.DTOs;

namespace CodeCoach.Infrastructure.Auth;

public class RefreshTokenProvider : IRefreshTokenProvider
{
    private readonly RefreshTokenOptions _options;

    public RefreshTokenProvider(IOptions<RefreshTokenOptions> options)
    {
        _options = options.Value;
    }

    public RefreshTokenDescriptorDto Generate()
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        return new RefreshTokenDescriptorDto(
            token,
            ComputeHash(token),
            DateTime.UtcNow.AddDays(_options.ExpirationDays));
    }

    public string ComputeHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
