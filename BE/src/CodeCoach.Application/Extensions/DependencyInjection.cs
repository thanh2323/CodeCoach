using MediatR;

using Microsoft.Extensions.DependencyInjection;

using CodeCoach.Application.Abstractions;
using CodeCoach.Application.Rooms.Commands.CreateRoom;
using CodeCoach.Application.Services;

namespace CodeCoach.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<CreateRoomCommandHandler>());

        services.AddSingleton<IJoinCodeGenerator, JoinCodeGenerator>();

        return services;
    }
}
