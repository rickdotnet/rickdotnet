using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration.Json;

namespace Vault.Extensions.Microsoft.Configuration;

public class VaultConfigurationProvider : JsonConfigurationProvider
{
    private readonly VaultConfigurationSource kvSource;

    public VaultConfigurationProvider(VaultConfigurationSource source) : base(source)
    {
        kvSource = source;
    }

    public override void Load()
    {
        var vaultUrl = kvSource.VaultOptions.VaultAddress;
        var vaultToken = kvSource.VaultOptions.VaultToken;
        var configKey = kvSource.VaultOptions.ConfigKey;

        var uri = new Uri($"{vaultUrl.TrimEnd('/')}/{configKey}");
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", vaultToken);

        var httpClient = new HttpClient();
        var response = httpClient.Send(request);
        var json = response.IsSuccessStatusCode
            ? response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            : null;
        
        if (string.IsNullOrEmpty(json))
        {
            Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            return;
        }

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        Load(stream);
    }
}
