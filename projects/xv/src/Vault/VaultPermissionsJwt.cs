using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using NATS.Jwt.Models;

namespace Vault;

public record VaultPermissionsJwt : JwtClaimsData
{
    [JsonPropertyName("permissions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyOrder(1)]
    public List<string> Permissions { get; set; } = [];
}

