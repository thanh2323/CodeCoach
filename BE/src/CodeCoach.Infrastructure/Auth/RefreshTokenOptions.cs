namespace CodeCoach.Infrastructure.Auth;

public class RefreshTokenOptions
{
    public int ExpirationDays { get; set; } = 7;
}
