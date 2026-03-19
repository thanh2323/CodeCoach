using System;

namespace CodeCoach.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    public string? CreatedByIp { get; private set; }

    private RefreshToken()
    {
        TokenHash = string.Empty;
    }

    public RefreshToken(
        Guid userId,
        string tokenHash,
        DateTime expiresAtUtc,
        string? createdByIp = null)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException("TokenHash is required.", nameof(tokenHash));
        }

        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
        CreatedByIp = createdByIp;
    }

    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;

    public void Revoke(Guid? replacedByTokenId = null)
    {
        if (RevokedAtUtc is not null)
        {
            return;
        }

        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
