using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MediatR;

using CodeCoach.Application.Abstractions;
using CodeCoach.Application.Interfaces;
using CodeCoach.Application.Rooms.Commands.CreateRoom;
using CodeCoach.Application.Services;
using CodeCoach.Infrastructure.Data;
using CodeCoach.Infrastructure.Data.Repositories;

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

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IRoomParticipantRepository, RoomParticipantRepository>();
        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<CreateRoomCommandHandler>());

        services.AddSingleton<IJoinCodeGenerator, JoinCodeGenerator>();

        return services;
    }
}
