namespace Vault.Web.Models;

public record UserSignals
{
    public bool SignedIn => !string.IsNullOrEmpty(PublicKey);
    public string? PublicKey { get; init; }
    public string? DefaultVault { get; init; }
    public string[] OpenVaults { get; init; } = [];
    public string[] CommandList { get; init; } = [];
    public string CurrentCommand { get; init; } = string.Empty;
    public bool SubmittingCommand { get; init; }
    public string? ErrorMessage { get; init; }
}
