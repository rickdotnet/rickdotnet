namespace Vault.Extensions.Microsoft.Configuration;

public record VaultConfigurationOptions
{
    public string VaultAddress { get; init; } = "https://localhost:8200/kv/my-vault";
    public string VaultToken { get; init; } = string.Empty;
    public string ConfigKey { get; init; } = "my-config";
}
