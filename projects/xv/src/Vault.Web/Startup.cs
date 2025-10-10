using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using NATS.Client.Core;
using NATS.Client.KeyValueStore;
using NATS.Extensions.Microsoft.DependencyInjection;
using NATS.Net;
using Serilog;
using Serilog.Events;
using StarFederation.Datastar.DependencyInjection;
using Vault.Web.Components;
using Vault.Web.Routing;

namespace Vault.Web;

public static class Startup
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        VaultSettings settings = builder
            .WithLogging()
            .WithConfig();

        builder.Services
            .AddRazorComponents();
        
        builder.Services.AddDatastar();
        builder.Services.AddScoped(sp => new HtmlRenderer(sp, sp.GetRequiredService<ILoggerFactory>()));
        builder.Services
            //.AddAuthentication("AuthHandler")
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie()
            .AddScheme<AuthenticationSchemeOptions, AuthHandler>("AuthHandler", null);

        builder.Services
            .AddAuthorizationBuilder()
            .AddPolicy("vault.read", policy =>
                policy.RequireAssertion(context =>
                {
                    var httpContext = context.Resource as HttpContext;
                    var vaultName = httpContext?.Request.RouteValues["vault"]?.ToString();
                    var keyName = httpContext?.Request.RouteValues["key"]?.ToString();

                    if (httpContext is null || string.IsNullOrEmpty(vaultName))
                        return false;

                    var hasReader = httpContext.User.HasVaultReader(vaultName);
                    if (!hasReader && !string.IsNullOrEmpty(keyName))
                        hasReader = httpContext.User.HasKeyReader(vaultName, keyName);

                    return hasReader;
                })
            )
            .AddPolicy("vault.write", policy =>
                policy.RequireAssertion(context =>
                {
                    var httpContext = context.Resource as DefaultHttpContext;
                    var vaultName = httpContext?.Request.RouteValues["vault"]?.ToString();
                    var keyName = httpContext?.Request.RouteValues["key"]?.ToString();

                    if (httpContext is null || string.IsNullOrEmpty(vaultName))
                        return false;

                    var hasWriter = httpContext.User.HasVaultWriter(vaultName);
                    if (!hasWriter && !string.IsNullOrEmpty(keyName))
                        hasWriter = httpContext.User.HasKeyWriter(vaultName, keyName);

                    return hasWriter;
                })
            )
            .AddPolicy("vault.admin", policy =>
                policy.RequireAssertion(context =>
                {
                    var httpContext = context.Resource as HttpContext;
                    var vaultName = httpContext?.Request.RouteValues["vault"]?.ToString();
                    var keyName = httpContext?.Request.RouteValues["key"]?.ToString();

                    if (httpContext is null || string.IsNullOrEmpty(vaultName))
                        return false;

                    var hasAdmin = httpContext.User.HasVaultAdmin(vaultName);
                    if (!hasAdmin && !string.IsNullOrEmpty(keyName))
                        hasAdmin = httpContext.User.HasKeyAdmin(vaultName, keyName);

                    return hasAdmin;
                })
            );

        builder.Services
            .AddAuthorization();

        builder.Services
            .AddNatsClient(nats => nats.ConfigureOptions(opts => opts with
                {
                    Url = settings.NatsUrl,
                    AuthOpts = new NatsAuthOpts
                    {
                        Username = settings.NatsUser,
                        Password = settings.NatsPass
                    }
                })
            )
            .AddSingleton<INatsKVContext>(p => p.GetRequiredService<NatsConnection>().CreateKeyValueStoreContext());


        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseAntiforgery();
        app.MapStaticAssets(); 
        //app.MapFallbackToFile("index.html");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapApi();
        app.MapApp();
        app.MapDataStar();

        return app;
    }

    private static VaultSettings WithConfig(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentVariables("VAULT_");
        builder.Configuration.AddJsonFile("vaultSettings.json", optional: true);
        builder.Services.Configure<VaultSettings>(builder.Configuration);
        builder.Services.AddTransient<VaultSettings>(x => x.GetRequiredService<IOptions<VaultSettings>>().Value);

        return builder.Configuration.Get<VaultSettings>()!;
    }

    private static WebApplicationBuilder WithLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                LogEventLevel.Debug,
                "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
            .CreateLogger();

        builder.Services.AddSerilog();

        return builder;
    }
}
