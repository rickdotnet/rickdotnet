namespace Dusty.Shared;

public record AppConfig
{
    public string GroqApiKey { get; set; } = Secrets.GroqApiKey;
    public string GitHubApiKey { get; set; } = Secrets.GitHubApiKey;
    public string NatsUrl { get; set; } = Secrets.NatsUrl;
    public string NatsUser { get; set; } = "dusty";
    public string NatsPassword { get; set; } = Secrets.NatsToken;
    public string DustyWorkingDirectory 
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dusty");
    public string DocumentsPath 
        => Path.Combine(DustyWorkingDirectory, "Documents");
}
