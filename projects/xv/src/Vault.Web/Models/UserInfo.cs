namespace Vault.Web.Models;

public record UserInfo
{
    public string PublicKey { get; init; }
    public string? DefaultVault { get; init; }
    
    public UserInfo(string publicKey, string? defaultVault = null)
    {
        PublicKey = publicKey;
        DefaultVault = defaultVault;
    }
}
