using Portal.Web.Api;
using Portal.Web.Api.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IUserStore, InMemoryUserStore>();

var app = builder.Build();
app.MapStaticAssets();
app.MapFallbackToFile("index.html");

app.MapGet("/api", () => "Hello, World! - portal");
app.MapV3();

app.Run();
