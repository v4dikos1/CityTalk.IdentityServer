using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Persistence;

namespace WebApi.StartupConfigurations;

public static class AspIdentityConfiguration
{
    public static IServiceCollection AddAspIdentityConfig(this IServiceCollection services, int passwordLengthRequired)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
            {
                opts.Password.RequiredLength = passwordLengthRequired;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = true;
            })
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();
        return services;
    }
}