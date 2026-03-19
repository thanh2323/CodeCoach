namespace CodeCoach.Api.Contracts.Auth;

public record AuthenticatedUserResponse(Guid Id, string Name, string Email);
