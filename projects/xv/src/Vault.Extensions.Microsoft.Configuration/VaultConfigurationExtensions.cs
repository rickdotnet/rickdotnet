using Microsoft.Extensions.Configuration;

namespace Vault.Extensions.Microsoft.Configuration;

public static class VaultConfigurationExtensions
{
    public static IConfigurationBuilder AddJsonFromVault(this IConfigurationBuilder builder, VaultConfigurationOptions options)
        => builder.Add(new VaultConfigurationSource(options));
}