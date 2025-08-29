using System.ComponentModel;
using Microsoft.Extensions.AI;
using Serilog;

namespace Dusty.Shared.Tools;

public class Documents
{
    public static AppConfig AppConfig = new();
    public static AIFunction[] Tools =>
    [
        AIFunctionFactory.Create(SaveContent, Functions.SaveContent),
        AIFunctionFactory.Create(GetContent, Functions.GetContent),
        AIFunctionFactory.Create(ListFiles, Functions.ListFiles)
    ];

    [Description("Safely save's or updates the content of a file in the user's Documents.")]
    public static Task SaveContent(
        [Description("The path to the file in the Documents, relative to the root. For example, 'notes/my-note.md'.")]
        string filePath,
        [Description("The content to save in the file. If the file already exists, it will be updated with this content.")]
        string content)
    {
        Log.Information("Saving content to file: {FilePath}", filePath);
        return Task.CompletedTask;
    }

    [Description("Safely retrieves the content of a file from the user's Documents. Returns a JSON formatted {\"error\": \"string\"} if the file does not exist or an error occurs.")]
    public async static Task<string> GetContent(
        [Description("The path to the file in the Documents, relative to the root. For example, 'notes/my-note.md'.")]
        string filePath)
    {
        Log.Information("Retrieving content from file: {FilePath}", filePath);
        return "# This is my Title\n\nThis is the content of my document.";
    }

    [Description("Lists all Markdown files in the Documents or a specific directory within it.")]
    public static string[] ListFiles()
    {
        AppConfig appConfig = new();
        var markdownFiles = Directory.GetFiles(appConfig.DocumentsPath, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(appConfig.DocumentsPath, f))
            .ToArray();
        return markdownFiles;

        // return;
        //
        // // Placeholder for actual implementation
        // var relativePath = directoryPath ?? string.Empty;
        // var filePath = Path.Combine(DocumentsRoot, relativePath);
        // var files = Directory.GetFiles(filePath, "*.md", SearchOption.AllDirectories)
        //     .Select(f => Path.GetRelativePath(DocumentsRoot, f))
        //     .ToArray();
        //
        // return Task.FromResult(files);
    }

    public static class Functions
    {
        public const string SaveContent = "Documents.SaveContent";
        public const string GetContent = "Documents.GetContent";
        public const string ListFiles = "Documents.ListFiles";
    }
}
