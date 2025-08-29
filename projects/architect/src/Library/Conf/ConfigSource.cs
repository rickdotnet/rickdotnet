using Microsoft.Extensions.Configuration;

namespace Library.Conf;

public class ConfigurationSource : IConfigurationSource
{
    private readonly string filePath;

    public ConfigurationSource(string filePath)
    {
        this.filePath = filePath;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new ConfFileProvider(filePath);
    }
}

public sealed class ConfFileProvider : ConfigurationProvider, IDisposable
{
    private readonly string filePath;
    private FileSystemWatcher watcher;

    public ConfFileProvider(string filePath)
    {
        this.filePath = filePath;
        CreateDefaultConfig(filePath);

        watcher = new FileSystemWatcher(Path.GetDirectoryName(filePath) ?? string.Empty)
        {
            Filter = Path.GetFileName(filePath),
            NotifyFilter = NotifyFilters.LastWrite
        };

        watcher.Changed += (_, _) => ReloadConfig();
        watcher.EnableRaisingEvents = true;
    }
    
    private static void CreateDefaultConfig(string filePath)
    {
        if (File.Exists(filePath))
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);
        File.WriteAllText(filePath, DefaultConfig.ConfFile);
    }

    public override void Load()
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in File.ReadAllLines(filePath))
        {
            if (string.IsNullOrEmpty(line) || line.StartsWith('#') || line.StartsWith("//"))
                continue;

            var parts = line.Split('=', StringSplitOptions.TrimEntries);
            var key = parts[0];
            var value = parts.Length > 1 ? string.Join("=", parts[1..]) : string.Empty;

            var nullOrEmpty = string.IsNullOrEmpty(value);
            var containsKey = data.ContainsKey(key);

            if (nullOrEmpty)
                Console.WriteLine($"Key '{key}' has no value. I hope that's what you wanted.");

            if (containsKey)
                Console.WriteLine($"Duplicate key found: {key}. Overwriting value.");

            data[key] = value;
        }

        Data = data;
    }

    private void ReloadConfig()
    {
        Load();
        OnReload();
    }


    public void Dispose() => watcher.Dispose();

}

public static class DefaultConfig
{
    public static string ConfFile =>
        """
        # Default configuration
        BashAlias = arch
        Database:Provider = DefaultProvider
        Logging:Level = Information
        Logging:FilePath = /var/log/architect.log
        """;
}
