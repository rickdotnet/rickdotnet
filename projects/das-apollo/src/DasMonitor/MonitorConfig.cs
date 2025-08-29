namespace DasMonitor;

public record MonitorConfig
{
    public string NatsUrl { get; init; } = "nats://localhost:4222";
    
    public string? Username { get; init; }

    public string? Password { get; init; }

    public string? Token { get; init; }

    public string? Jwt { get; init; }

    public string? NKey { get; init; }

    public string? Seed { get; init; }

    public string? CredsFile { get; init; }

    public string? NKeyFile { get; init; }
}