var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseIdentityServer();

app.Run();
