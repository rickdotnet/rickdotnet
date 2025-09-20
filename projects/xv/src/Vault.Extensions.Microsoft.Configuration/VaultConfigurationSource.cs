using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Vault.Extensions.Microsoft.Configuration;

public class VaultConfigurationSource : JsonConfigurationSource
{
    public VaultConfigurationOptions VaultOptions { get; }

    public VaultConfigurationSource(VaultConfigurationOptions options)
    {
        VaultOptions = options;
        Optional = true;
    }

    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new VaultConfigurationProvider(this);
    }
}
