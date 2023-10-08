using Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Configs;

namespace Persistence;

public static class ServiceRegistrar
{
    public static IServiceCollection RegisterDataAccessServices(
        this IServiceCollection services, string connectionString, bool isDevelopment)
    {
        services.AddDbContext<IdentityContext>(options =>
        {
            options.UseNpgsql(connectionString);
            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging();
            }
        });
        return services;
    }

    public static IApplicationBuilder MigrateDb(this IApplicationBuilder app, IConfiguration configuration)
    {
        using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
        if (serviceScope == null)
        {
            return app;
        }

        serviceScope.ServiceProvider.GetRequiredService<CustomPersistedGrantDbContext>().Database.Migrate();
        var identityContext = serviceScope.ServiceProvider.GetRequiredService<IdentityContext>();
        var configurationContext = serviceScope.ServiceProvider.GetRequiredService<CustomConfigurationDbContext>();
        var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        identityContext.Database.Migrate();
        configurationContext.Database.Migrate();
        var seed = configuration.GetValue<bool>("NeedSeedData");
        if (seed)
        {
            SeedData.RecreateClients(configuration, configurationContext);
            SeedData.RecreateApiResources(configurationContext);
            SeedData.RecreateIdentityResources(configurationContext);
            SeedData.RecreateApiScopes(configurationContext);
            SeedData.IdentityRoleSeed(roleManager);
            SeedData.AdminUserSeed(userManager);
        }

        return app;
    }
}