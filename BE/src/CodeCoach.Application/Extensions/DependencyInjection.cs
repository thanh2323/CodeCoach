using MediatR;

using Microsoft.Extensions.DependencyInjection;

using CodeCoach.Application.Abstractions;
using CodeCoach.Application.Features.Rooms.Commands.CreateRoom;
using CodeCoach.Application.Features.Rooms.Services;

namespace CodeCoach.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<CreateRoomCommandHandler>());

        services.AddScoped<ICreateRoomService, RoomCreationService>();
        services.AddScoped<IJoinRoomService, RoomJoinService>();

        return services;
    }
}
