using Apollo.Extensions.Microsoft.Hosting;
using Apollo.Providers.NATS;
using DasMonitor.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using Serilog;
using Serilog.Events;

namespace DasMonitor;

public static class Startup
{
    public static IHost ConfigureHost(this HostApplicationBuilder builder)
    {
        builder.AddLogging();
        var monitorConfig = builder.AddConfig();

        builder.Services.AddHttpClient<DasSignalClient>();
        builder.Services.AddSingleton<DasMonitorEndpoint>();
        builder.Services.AddSingleton<DasPublisher>();
        builder.Services.AddApollo(ab => ab
            .AddEndpoint<DasMonitorEndpoint>(DasMonitorConstants.EndpointConfig)
            .AddNatsProvider(opts => opts with
            {
                Url = monitorConfig.NatsUrl,
                AuthOpts = new NatsAuthOpts
                {
                    Username = monitorConfig.Username,
                    Password = monitorConfig.Password,
                    Token = monitorConfig.Token,
                    Jwt = monitorConfig.Jwt,
                    NKey = monitorConfig.NKey,
                    Seed = monitorConfig.Seed,
                    CredsFile = monitorConfig.CredsFile,
                    NKeyFile = monitorConfig.NKeyFile
                }
            })
        );

        return builder.Build();
    }

    private static void AddLogging(this HostApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("NATS.Client.Core.Internal", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                LogEventLevel.Debug,
                "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
            .CreateLogger();

        builder.Services.AddSerilog();
    }

    private static MonitorConfig AddConfig(this HostApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("monitorConfig.json", optional: true);
        builder.Configuration.AddEnvironmentVariables("DM_");

        // add the app config to the service collection
        var config = builder.Configuration.Get<MonitorConfig>() ?? throw new Exception("MonitorConfig is null");
        builder.Services.AddSingleton(config);

        return config;
    }
}