using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

namespace RickConsole;

public static class Startup
{
    public static IHost ConfigureHost(this HostApplicationBuilder builder)
    {
        builder.AddLogging();

        var config = builder.AddConfig();

        return builder.Build();
    }

    private static AppConfig AddConfig(this HostApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentVariables("APP_");
        builder.Configuration.AddJsonFile("appConfig.json", optional: true);
        builder.Services.Configure<AppConfig>(builder.Configuration);
        builder.Services.AddTransient<AppConfig>(x => x.GetRequiredService<IOptions<AppConfig>>().Value);

        return builder.Configuration.Get<AppConfig>()!;
    }

    private static void AddLogging(this HostApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                LogEventLevel.Debug,
                "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
            .CreateLogger();
    }
}
