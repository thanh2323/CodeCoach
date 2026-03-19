using CodeCoach.Application.DTOs;
using CodeCoach.Domain.Entities;

namespace CodeCoach.Application.Abstractions;

public interface IJwtTokenGenerator
{
    JwtTokenDto Generate(User user);
}
