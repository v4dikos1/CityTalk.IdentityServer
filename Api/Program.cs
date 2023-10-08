using Microsoft.AspNetCore.HttpOverrides;
using Persistence;
using System.Reflection;
using WebApi.StartupConfigurations;

//var assembly = Assembly.Load("Infrast");
var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var connectionString = builder.Configuration.GetConnectionString("CityTalkIdentityContext");

builder.Services.RegisterDataAccessServices(connectionString!, builder.Environment.IsDevelopment());
builder.Services.AddIdentityServerConfig(connectionString!);

const int passwordLengthRequired = 8;
builder.Services.AddAspIdentityConfig(passwordLengthRequired);
builder.Services.AddAuthenticationConfig(builder.Configuration);
builder.Services.AddCorsConfig();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddLocalApiAuthentication();
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowAllOrigins");

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

var forwardOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    RequireHeaderSymmetry = false
};

forwardOptions.KnownNetworks.Clear();
forwardOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardOptions);
app.MigrateDb(builder.Configuration);
app.UseStaticFiles();
app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
app.UseRouting();

app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});

app.Run();