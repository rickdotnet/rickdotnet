namespace Library.Conf;

public record AppConfig
{
    public static string DefaultConfFile =>
        """
        # Default configuration
        BashAlias = arch
        LoggingLevel = Information
        LoggingFilePath = /var/log/architect.log
        """;
    
    public string BashAlias { get; init; } = "arch";
    public string LoggingLevel { get; init; } = "Warning";
    public string LoggingFilePath { get; init; } = "/var/log/architect.log";
}