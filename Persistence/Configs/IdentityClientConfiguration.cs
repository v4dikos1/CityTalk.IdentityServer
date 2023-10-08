namespace Persistence.Configs;

public class IdentityClientConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public int AccessTokenLifetime { get; set; }
}