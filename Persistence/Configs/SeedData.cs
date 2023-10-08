using Domain.Entities;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.Extensions.Configuration;

namespace Persistence.Configs;

public class SeedData
{
    private const string AdminId = "bb313033-6c2d-4add-a021-9152d78b0c70";

    //Resource
    private const string CityTalkResourceName = "CityTalk.Backend.Api";

    //custom scopes
    private const string CityTalkScopeName = "CityTalk.Api.Scope";

    private const string UserIsActivatedScopeName = "user_is_activated";
    private const string RoleScopeName = "roles";

    //server clients
    private const string CityTalkServerClientName = "CityTalk.ServerClient";
    private const string IdentityServerClientName = "Identity.ServerClient";

    //swagger clients
    private const string CityTalkSwaggerClientName = "CityTalk.SwaggerClient";

    //frontend clients
    private const string CityTalkWebJsClientName = "CityTalk.WebJsClient";
    private const string CityTalkMobileClientName = "CityTalk.MobileClient";

    //Roles
    private const string SuperAdminRole = "super_admin";
    private const string EventCreatorRole = "event_creator";
    private const string EventParticipantRole = "event_participant";


    private static readonly string[] InitialRoles =
    {
        SuperAdminRole, EventCreatorRole, EventParticipantRole
    };


    private static IEnumerable<IdentityResource> IdentityResources =>
        new[]
        {
            new IdentityResources.OpenId(), new IdentityResources.Profile(), new IdentityResources.Email(),
            new IdentityResources.Phone(),
            new() { Name = RoleScopeName, DisplayName = "Roles", UserClaims = { JwtClaimTypes.Role } },
            new IdentityResource
            {
                Name = UserIsActivatedScopeName,
                DisplayName = "User is activated",
                UserClaims = { UserIsActivatedScopeName }
            }
        };

    private static IEnumerable<ApiScope> PermanentApiScopes =>
        new[]
        {
            new ApiScope(IdentityServerConstants.LocalApi.ScopeName),
            new ApiScope(CityTalkScopeName)
        };

    private static IEnumerable<ApiResource> ApiResources =>
        new[]
        {
            new ApiResource(CityTalkResourceName, "City talk backend API", new[] { JwtClaimTypes.Role })
            {
                Scopes =
                {
                    CityTalkScopeName
                }
            },
            new ApiResource(IdentityServerConstants.LocalApi.ScopeName)
        };

    public static void RecreateClients(IConfiguration configuration, CustomConfigurationDbContext context)
    {
        var permanentClients = GetPermanentClientIdList();
        var existedClients = context.Clients.ToList();
        var removeClients = existedClients.Where(x => permanentClients.Contains(x.ClientId));
        context.Clients.RemoveRange(removeClients);
        context.SaveChanges();
        var clients = new List<Client>
        {
            GetSwaggerClient(configuration, CityTalkSwaggerClientName, new[]
            {
                CityTalkScopeName, RoleScopeName, UserIsActivatedScopeName
            }),

            GetWebJsClient(configuration, CityTalkWebJsClientName, new[]
            {
                CityTalkScopeName, RoleScopeName, UserIsActivatedScopeName
            }),

            GetMobileClient(configuration, CityTalkMobileClientName, new []
            {
                CityTalkScopeName, RoleScopeName, UserIsActivatedScopeName
            }),

            GetServersideClient(configuration, CityTalkServerClientName, new[]
            {
                CityTalkScopeName, RoleScopeName
            }),

            GetServersideClient(configuration, IdentityServerClientName, new[]
            {
                CityTalkScopeName
            }),
        };
        context.Clients.AddRange(clients.Select(x => x.ToEntity()));
        context.SaveChanges();
    }

    public static void RecreateApiResources(CustomConfigurationDbContext context)
    {
        var apiResources = ApiResources.Select(x => x.ToEntity());
        var removeApiResources = context.ApiResources.Where(x => true);
        context.ApiResources.RemoveRange(removeApiResources);
        context.SaveChanges();
        context.ApiResources.AddRange(apiResources);
        context.SaveChanges();
    }

    public static void RecreateIdentityResources(CustomConfigurationDbContext context)
    {
        var identityResources = IdentityResources.Select(x => x.ToEntity());
        var removeIdentityResources = context.IdentityResources.Where(x => true);
        context.IdentityResources.RemoveRange(removeIdentityResources);
        context.SaveChanges();
        context.IdentityResources.AddRange(identityResources);
        context.SaveChanges();
    }

    public static void RecreateApiScopes(CustomConfigurationDbContext context)
    {
        var apiScopes = PermanentApiScopes.Select(x => x.ToEntity()).ToList();
        var removeApiScopes = context.ApiScopes.Where(x => true);
        context.ApiScopes.RemoveRange(removeApiScopes);
        context.SaveChanges();
        context.ApiScopes.AddRange(apiScopes);
        context.SaveChanges();
    }

    public static void IdentityRoleSeed(RoleManager<IdentityRole> roleManager)
    {
        foreach (var roleName in InitialRoles)
        {
            var role = roleManager.FindByNameAsync(roleName).Result;
            if (role == null)
            {
                role = new IdentityRole { Name = roleName };
                _ = roleManager.CreateAsync(role).Result;
            }
        }
    }

    public static void AdminUserSeed(UserManager<ApplicationUser> userManager)
    {
        var adminUserName = "+77777777777";
        var password = "Qwerty12345";
        var admin = userManager.FindByIdAsync(AdminId).Result;
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                Id = AdminId,
                Email = "v4dikos@yandex.ru",
                EmailConfirmed = true,
                PhoneNumber = adminUserName,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                UserName = adminUserName,
                Surname = "Админов",
                Name = "Админ",
                Patronymic = "Админович"
            };
            var result = userManager.CreateAsync(admin, password).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userManager.AddClaimsAsync(admin,
                new Claim[]
                {
                    new(JwtClaimTypes.Name, admin.Name), new(JwtClaimTypes.GivenName, admin.Patronymic),
                    new(JwtClaimTypes.FamilyName, admin.Surname),
                    new(JwtClaimTypes.PhoneNumber, admin.PhoneNumber ?? string.Empty),
                    new(JwtClaimTypes.PhoneNumberVerified, admin.PhoneNumberConfirmed.ToString()),
                    new(JwtClaimTypes.Email, admin.Email ?? string.Empty),
                    new(JwtClaimTypes.EmailVerified, admin.EmailConfirmed.ToString())
                }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            if (!userManager.IsInRoleAsync(admin, "super_admin").Result)
            {
                _ = userManager.AddToRoleAsync(admin, "super_admin").Result;
            }
        }
    }


    private static Client GetSwaggerClient(IConfiguration configuration, string clientId,
        IEnumerable<string> userAllowedScopes)
    {
        var configKey = clientId.Split('.').Aggregate(string.Empty, (accumulate, next) => accumulate + next);
        var clientName = clientId.Split('.').Aggregate(string.Empty, (accumulate, next) => accumulate + next + ' ')
            .TrimEnd();
        var config = configuration.GetSection($"Clients:{configKey}").Get<IdentityClientConfiguration>();
        var allowCorsOrigins = config.BaseUrl.Split(";").ToList();
        var redirectUris = allowCorsOrigins.Select(x => $"{x}/swagger/oauth2-redirect.html").ToList();
        var postLogoutRedirectUris = allowCorsOrigins.Select(x => $"{x}/swagger/index.html").ToList();
        var allowedScopes = new List<string>
        {
            IdentityServerConstants.StandardScopes.OpenId,
            IdentityServerConstants.StandardScopes.Profile,
            IdentityServerConstants.StandardScopes.Phone,
            IdentityServerConstants.StandardScopes.Email
        };
        allowedScopes.AddRange(userAllowedScopes);
        return new Client
        {
            ClientId = clientId,
            ClientSecrets = { new Secret(config.ClientSecret.Sha256()) },
            EnableLocalLogin = true,
            ClientName = clientName,
            AllowedGrantTypes = GrantTypes.Code,
            RequirePkce = true,
            RequireClientSecret = false,
            AlwaysSendClientClaims = true,
            RequireConsent = false,
            AllowAccessTokensViaBrowser = true,
            AlwaysIncludeUserClaimsInIdToken = true,
            AllowOfflineAccess = true,
            RedirectUris = redirectUris,
            PostLogoutRedirectUris = postLogoutRedirectUris,
            AllowedCorsOrigins = allowCorsOrigins,
            AccessTokenLifetime = config.AccessTokenLifetime,
            AllowedScopes = allowedScopes
        };
    }

    private static Client GetWebJsClient(IConfiguration configuration, string clientId,
        IEnumerable<string> userAllowedScopes)
    {
        var configKey = clientId.Split('.').Aggregate(string.Empty, (accumulate, next) => accumulate + next);
        var clientName = clientId.Split('.').Aggregate(string.Empty, (accumulate, next) => accumulate + next + ' ')
            .TrimEnd();
        var config = configuration.GetSection($"Clients:{configKey}").Get<IdentityClientConfiguration>();
        var allowCorsOrigins = config.BaseUrl.Split(";").ToList();
        var allowedScopes = new List<string>
        {
            IdentityServerConstants.StandardScopes.OpenId,
            IdentityServerConstants.StandardScopes.Profile,
            IdentityServerConstants.StandardScopes.Phone,
            IdentityServerConstants.StandardScopes.Email
        };
        allowedScopes.AddRange(userAllowedScopes);

        return new Client
        {
            ClientId = clientId,
            ClientSecrets = { new Secret(config.ClientSecret.Sha256()) },
            EnableLocalLogin = true,
            ClientName = clientName,
            AllowedGrantTypes = GrantTypes.Code,
            RequirePkce = true,
            RequireClientSecret = false,
            AlwaysSendClientClaims = true,
            RequireConsent = false,
            AllowAccessTokensViaBrowser = true,
            AccessTokenLifetime = config.AccessTokenLifetime,
            AllowOfflineAccess = true,
            AlwaysIncludeUserClaimsInIdToken = true,
            RedirectUris = allowCorsOrigins,
            PostLogoutRedirectUris = allowCorsOrigins,
            AllowedCorsOrigins = allowCorsOrigins,
            AbsoluteRefreshTokenLifetime = 5184000,
            AllowedScopes = allowedScopes
        };
    }

    private static Client GetMobileClient(IConfiguration configuration, string clientId,
        IEnumerable<string> userAllowedScopes)
    {
        var configKey = clientId.Split('.').Aggregate(string.Empty, (accumulate, next) => accumulate + next);
        var clientName = clientId.Split('.').Aggregate(string.Empty, (accumulate, next) => accumulate + next + ' ')
            .TrimEnd();
        var config = configuration.GetSection($"Clients:{configKey}").Get<IdentityClientConfiguration>();
        var allowCorsOrigins = config.BaseUrl.Split(";").ToList();
        var allowedScopes = new List<string>
        {
            IdentityServerConstants.StandardScopes.OpenId,
            IdentityServerConstants.StandardScopes.Profile,
            IdentityServerConstants.StandardScopes.Phone,
            IdentityServerConstants.StandardScopes.Email
        };
        allowedScopes.AddRange(userAllowedScopes);

        return new Client
        {
            ClientId = clientId,
            ClientSecrets = { new Secret(config.ClientSecret.Sha256()) },
            EnableLocalLogin = true,
            ClientName = clientName,
            AllowedGrantTypes = GrantTypes.Code,
            RequirePkce = true,
            RequireClientSecret = false,
            AlwaysSendClientClaims = true,
            RequireConsent = false,
            AllowAccessTokensViaBrowser = true,
            AccessTokenLifetime = config.AccessTokenLifetime,
            AllowOfflineAccess = true,
            AlwaysIncludeUserClaimsInIdToken = true,
            RedirectUris = allowCorsOrigins,
            PostLogoutRedirectUris = allowCorsOrigins,
            AllowedCorsOrigins = null,
            AbsoluteRefreshTokenLifetime = 5184000,
            AllowedScopes = allowedScopes
        };
    }

    private static Client GetServersideClient(IConfiguration configuration, string clientId,
        IEnumerable<string> userScopes)
    {
        var configKey = clientId.Split('.').Aggregate(string.Empty, (accumulate, next) => accumulate + next);
        var clientName = clientId.Split('.').Aggregate(string.Empty, (accumulate, next) => accumulate + next + ' ')
            .TrimEnd();
        var config = configuration.GetSection($"Clients:{configKey}").Get<IdentityClientConfiguration>();
        return new Client
        {
            ClientId = clientId,
            ClientSecrets = { new Secret(config.ClientSecret.Sha256()) },
            ClientName = clientName,
            AccessTokenLifetime = config.AccessTokenLifetime,
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            AllowedScopes = userScopes.ToList()
        };
    }

    public static IEnumerable<string> GetPermanentClientIdList()
    {
        return new List<string>
        {
            CityTalkServerClientName,
            CityTalkSwaggerClientName,
            CityTalkMobileClientName,
            CityTalkWebJsClientName,
            IdentityServerClientName
        };
    }
}