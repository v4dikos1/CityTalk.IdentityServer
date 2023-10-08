using Microsoft.EntityFrameworkCore;
using Persistence;

namespace WebApi.StartupConfigurations;

public static class IdentityServerConfiguration
{
    public static IServiceCollection AddIdentityServerConfig(this IServiceCollection services, string connectionString)
    {
        var identityServerBuilder = services.AddIdentityServer(options =>
        {
            options.Discovery.CustomEntries.Add("manage", "~/manage");
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;
            options.EmitStaticAudienceClaim = true;
        });
        identityServerBuilder.AddDeveloperSigningCredential();

        identityServerBuilder.AddConfigurationStore<CustomConfigurationDbContext>(options =>
        {
            options.ConfigureDbContext = b => b.UseNpgsql(connectionString,
                sql => sql.MigrationsAssembly("Persistence"));
        });
        identityServerBuilder.AddOperationalStore<CustomPersistedGrantDbContext>(options =>
        {
            options.ConfigureDbContext = b => b.UseNpgsql(connectionString,
                sql => sql.MigrationsAssembly("Persistence"));
        });
        //identityServerBuilder.AddAspNetIdentity<ApplicationUser>();
        identityServerBuilder.AddDeveloperSigningCredential();

        return services;
    }
}