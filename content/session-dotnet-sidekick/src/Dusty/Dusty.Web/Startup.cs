using Dusty.Shared.Hosting;
using Dusty.Web.Components;
using NATS.Client.Core;
using NATS.Extensions.Microsoft.DependencyInjection;

namespace Dusty.Web;

public static class Startup
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var config = builder.AddConfig();
        builder.AddLogging();
        builder.Services
            .AddHttpContextAccessor()
            .AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddNatsClient(nats => nats
            .ConfigureOptions(opts => opts with
            {
                Url = config.NatsUrl,
                AuthOpts = NatsAuthOpts.Default with
                {
                    Username = config.NatsUser,
                    Password = config.NatsPassword,
                },
            })
        );
        
        builder.Services.AddAI(config);
        
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
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        
        return app;
    }
}