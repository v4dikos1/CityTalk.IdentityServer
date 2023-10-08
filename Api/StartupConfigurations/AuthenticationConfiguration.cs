using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebApi.StartupConfigurations;

public static class AuthenticationConfiguration
{
    public static IServiceCollection AddAuthenticationConfig(this IServiceCollection services,
        IConfiguration configuration)
    {
        var authenticationBuilder = services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        return services;
    }
}