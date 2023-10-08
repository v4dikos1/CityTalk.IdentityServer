using Application.Users;
using Domain.Entities;
using Infrastructure.Services.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class ServiceRegistrar
{
    public static IServiceCollection RegisterInfrastructureServices(
        this IServiceCollection services, int passwordLengthRequired)
    {
        services.AddTransient<IUserService, UserService>();
        return services;
    }
}