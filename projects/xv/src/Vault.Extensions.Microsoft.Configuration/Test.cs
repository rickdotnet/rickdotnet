using Microsoft.Extensions.Configuration;

namespace Vault.Extensions.Microsoft.Configuration;

public class Test
{
    public void Main()
    {
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFromVault(new VaultConfigurationOptions
    {
        VaultAddress = "https://localhost:8200/kv/my-vault",
        VaultToken = "my-access-token",
        ConfigKey = ""
    }).Build();
    }
}
