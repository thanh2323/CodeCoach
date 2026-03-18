using CodeCoach.Application.Abstractions;

namespace CodeCoach.Application.Services;

public class JoinCodeGenerator : IJoinCodeGenerator
{
    public string Generate()
    {
        return Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
    }
}
