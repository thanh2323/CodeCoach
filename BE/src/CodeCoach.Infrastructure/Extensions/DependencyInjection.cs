using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using CodeCoach.Application.Abstractions;
using CodeCoach.Application.Interfaces;
using CodeCoach.Infrastructure.Auth;
using CodeCoach.Infrastructure.Data;
using CodeCoach.Infrastructure.Data.Repositories;
using CodeCoach.Infrastructure.Transactions;

namespace CodeCoach.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<RefreshTokenOptions>(configuration.GetSection("RefreshTokens"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IRoomParticipantRepository, RoomParticipantRepository>();
        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasherAdapter>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenProvider, RefreshTokenProvider>();
        services.AddScoped<ITransactionManager, TransactionManager>();

        return services;
    }
}
