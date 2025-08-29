using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using TextCopy;

namespace Dusty.Shared.Tools;

public class Gist
{
    public static AIFunction[] Tools =>
    [
        AIFunctionFactory.Create(CreateGistFromClipboardAsync, Functions.CreateGistFromClipboard),
        AIFunctionFactory.Create(CreateGistAsync, Functions.CreateGist)
    ];
    
    [Description("Creates a new service from the user's clipboard contents.")]
    public static async Task<string> CreateGistFromClipboardAsync(
        [Description("The description for the Gist. Defaults to 'Dusty Gist'.")]
        string description = "Dusty Gist"
    )
    {
        var text = await ClipboardService.GetTextAsync();
        if (!string.IsNullOrWhiteSpace(text))
            return await CreateGistAsync(text, description);

        Console.WriteLine("Clipboard is empty or contains only whitespace.");
        return "error";

    }

    [Description("Creates a Gist for the provided content.")]
    public static async Task<string> CreateGistAsync(string content, string description = "Dusty Gist")
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Secrets.GitHubApiKey);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Dusty/1.0");
        
        var gistJson = $@"{{
            ""description"": ""{content}"",
            ""public"": true,
            ""files"": {{
                ""test.txt"": {{
                    ""content"": ""Hello from .NET! This is a test gist.""
                }}
            }}
        }}";
        
        var requestContent = new StringContent(gistJson, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://api.github.com/gists", requestContent);
        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.GetProperty("html_url").GetString() ?? "error";
        }

        Console.WriteLine($"Failed to create gist: {response.StatusCode}");
        Console.WriteLine(await response.Content.ReadAsStringAsync());
            
        return "error";
    }
    
    public static class Functions
    {
        public const string CreateGistFromClipboard = "Gist.CreateGistFromClipboard";
        public const string CreateGist = "Gist.CreateGist";
    }
}
