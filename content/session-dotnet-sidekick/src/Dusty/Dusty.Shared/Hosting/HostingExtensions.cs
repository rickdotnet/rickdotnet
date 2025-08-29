using System.ClientModel;
using Aether.Abstractions.Hosting;
using Aether.Extensions.Microsoft.Hosting;
using Aether.Providers.NATS;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NATS.Client.Core;
using NATS.Extensions.Microsoft.DependencyInjection;
using OpenAI;
using Serilog;
using Serilog.Events;

namespace Dusty.Shared.Hosting;

public static class HostingExtensions
{
    public static AppConfig AddConfig(this IHostApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentVariables("APP_");
        builder.Configuration.AddJsonFile("appConfig.json", optional: true);
        builder.Services.Configure<AppConfig>(builder.Configuration);
        builder.Services.AddTransient<AppConfig>(x => x.GetRequiredService<IOptions<AppConfig>>().Value);
        
        return builder.Configuration.Get<AppConfig>()!;
    }

    public static void AddLogging(this IHostApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                LogEventLevel.Debug,
                "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
            .CreateLogger();

        builder.Services.AddSerilog();
    }

    public static void AddAI(this IServiceCollection services, AppConfig config)
    {
        var credential = new ApiKeyCredential(config.GitHubApiKey);
        var openAIOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://models.inference.ai.azure.com")
        };
        var ghModelsClient = new OpenAIClient(credential, openAIOptions);
        var client = ghModelsClient.GetChatClient("gpt-4o-mini").AsIChatClient();
        services.AddChatClient(client).UseFunctionInvocation();
    }

    public static void AddMessaging(this IServiceCollection services, AppConfig config, Action<IHubBuilder>? configureEndpoints = null)
    {
        // docker run -d -p 4222:4222 nats:latest -js
        services.AddNatsClient(nats => nats.ConfigureOptions(opts => opts with
                {
                    Url = config.NatsUrl,
                    AuthOpts = NatsAuthOpts.Default with
                    {
                        Username = config.NatsUser,
                        Token = config.NatsPassword,
                    },
                }
            )
        );
        
        configureEndpoints ??= _ => { };
        services.AddAether(ab =>
        {
            ab.Messaging
                .AddNatsHub(configureEndpoints);
        });
    }
}
