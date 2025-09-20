using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using NATS.Jwt.Models;
using NATS.NKeys;
using Spectre.Console;
using Vault;
using Vault.Extensions.Microsoft.Configuration;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFromVault(new VaultConfigurationOptions
    {
        VaultAddress = "http://localhost:5263/kv/appconfig-dev",
        VaultToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJlZDI1NTE5LW5rZXkifQ.eyJqdGkiOiJHQVhIUUNFSExINlpLV1VGRDNTUVhPVjZSUTdONTNPSkFRQTVBR1BKRTRBQUtIWU1QV0tRIiwiaWF0IjoxNzU0MjU5NzUyLCJpc3MiOiJBQjVNMlBGVUFFSUg3U04zUE9CV05DNjU2NlNTVUw1T0RLUFVBQTdGRFFZNUlBS00yRVVKV01aSSIsInN1YiI6IkFCNU0yUEZVQUVJSDdTTjNQT0JXTkM2NTY2U1NVTDVPREtQVUFBN0ZEUVk1SUFLTTJFVUpXTVpJIiwiZXhwIjoxNzcwMTU3MzUyLCJwZXJtaXNzaW9ucyI6WyJ2YXVsdDpyZWFkOmFwcGNvbmZpZy1kZXYiXX0.7gghxA0kIBXvvmz0nfZtOa2WC_-w1ZFAeUueiFt7UaPvaF153PjxMMh7qZXnEO89zGzm8877IDeFL6fbIo6NDA",
        ConfigKey = "foo"
    }).Build();

var value = configuration["bar"];
AnsiConsole.MarkupLine($"[green]Vault Config Value:[/] {value}");

return;

var settings = new VaultSettings();
var vaultPair = KeyPair.FromSeed("SAAALU7ARSDQVTULFDHD4MGHRCM2AWAZ4HAVWQTH5RSWUUPEB2ORJXDZRY");
AnsiConsole.MarkupLine($"Vault Issuer Key: [green]{vaultPair.GetPublicKey()}[/]");
AnsiConsole.MarkupLine($"Vault Issuer Seed: [green]{vaultPair.GetSeed()}[/]");

var userPair = KeyPair.CreatePair(PrefixByte.Account);
AnsiConsole.MarkupLine($"User Key: [green]{userPair.GetPublicKey()}[/]");
AnsiConsole.MarkupLine($"User Seed: [green]{userPair.GetSeed()}[/]");

var adminClaims = new VaultPermissionsJwt
{
    Subject = userPair.GetPublicKey(), // could be different from the signer
    Expires = DateTimeOffset.UtcNow.AddMonths(6),
    Permissions =
    [
        "vault:admin:appconfig-dev",
    ]
};

var readerClaims = new VaultPermissionsJwt
{
    Subject = userPair.GetPublicKey(), // could be different from the signer
    Expires = DateTimeOffset.UtcNow.AddMonths(6),
    Permissions =
    [
        "vault:read:appconfig-dev",
    ]
};

// vault signs the admin token
var adminToken = JwtUtil.Encode(adminClaims, vaultPair);
AnsiConsole.MarkupLine($"Vault Admin: [green]{adminToken}[/]");

// users mint their own reader token
var readerToken = JwtUtil.Encode(readerClaims, userPair);
AnsiConsole.MarkupLine($"Vault Reader: [green]{readerToken}[/]");

// users could optionally mint their own admin token once
// the vault is created and they're assigned as admin

// var vaultJwt = MintedVaultJwt.FromToken(adminToken);
// var rawClaims = JwtUtil.DecodeClaims<VaultPermissionsJwt>(adminToken).ValueOrDefault();
// var parsedClaims = JwtUtil.ParseClaims(rawClaims?.Data ?? new Dictionary<string, JsonNode>());

return;

var currentDirectory = Directory.GetCurrentDirectory();
var configPath = Path.Combine(currentDirectory, "vaultSettings.json");
if (!File.Exists(configPath))
{
    var userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    configPath = Path.Combine(userDirectory, ".vault", "vaultSettings.json");
}

VaultSettings vaultSettings = File.Exists(configPath)
    ? ConfigFromFile(configPath)
    : ConfigFromPrompt();

var saveConfig = AnsiConsole.Confirm("Save settings to file?");
if (saveConfig)
{
    Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
    var json = JsonSerializer.Serialize(vaultSettings, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(configPath, json);
    AnsiConsole.MarkupLine($"Settings saved to [green]{configPath}[/]");
}
else
{
    AnsiConsole.MarkupLine("Configuration not saved.");
}

return;

// var host = Host.CreateApplicationBuilder(args).ConfigureHost();
// host.Run();
static VaultSettings ConfigFromFile(string configPath)
{
    if (!File.Exists(configPath))
    {
        throw new FileNotFoundException($"Configuration file not found: {configPath}");
    }

    var json = File.ReadAllText(configPath);
    return JsonSerializer.Deserialize<VaultSettings>(json) ?? throw new InvalidOperationException("Failed to deserialize vaultSettings");
}

static VaultSettings ConfigFromPrompt()
    => new()
    {
        //IssuerSeed = AnsiConsole.Ask("Vault Seed", ""),
        NatsUrl = AnsiConsole.Ask("NATS URL", "nats://localhost:4222"),
        NatsUser = AnsiConsole.Ask("NATS User", "vaultuser"),
        NatsPass = AnsiConsole.Ask("NATS Password", "vaultpass")
    };
