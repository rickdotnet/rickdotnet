using Library.Conf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Library;

public static class HostExtensions
{
    public static HostApplicationBuilder AddConfig(this HostApplicationBuilder builder, string filePath)
    {
        builder.Configuration.AddConfFile(filePath);
        return builder;
    }

    public static IConfigurationBuilder AddConfFile(this IConfigurationBuilder builder, string? filePath = null)
    {
        filePath ??= Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "architect",
            "architect.conf"
        );

        return builder.Add(new ConfigurationSource(filePath));
    }

}
