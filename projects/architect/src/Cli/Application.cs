using Architect.Cli.Services;
using ConsoleAppFramework;
using Library.Conf;

namespace Architect.Cli;

/// <summary>
/// We know one thing... YOLO.
/// </summary>
public class Application
{
    private readonly AppConfig config;

    public Application(AppConfig config)
    {
        this.config = config;
    }
    /// <summary>
    /// Caution.
    /// </summary>
    /// <param name="yolo">Chad mode</param>
    /// <param name="user">User mode</param>
    [Command("")]
    public async Task Run(bool yolo = false, bool user = false)
    {
        Console.WriteLine($"alias - {config.BashAlias}");
        if (yolo)
        {
            Console.WriteLine("YOLO!!");
            await YoloInstaller.SudoInstall();
        }

        if (user)
        {
            await YoloInstaller.UserInstall();
        }
        else
        {
            Console.WriteLine("Do you even yolo, bro?");
        }
    }
}
public static class ConfigCopier
{
    public static async Task CopyConfigs(string sourceDir, string destDir)
    {
        Console.WriteLine($"Copying configs from {sourceDir} to {destDir}...");

        // Resolve tilde to home directory
        destDir = destDir.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        // Check if source directory exists
        if (!Directory.Exists(sourceDir))
        {
            Console.WriteLine($"Source directory {sourceDir} does not exist. Skipping config copy.");
            return;
        }

        // Copy directory recursively, overwriting existing files
        CopyDirectory(sourceDir, destDir, true);
        Console.WriteLine($"Configs copied to {destDir}");

        await Task.CompletedTask; // For async compatibility
    }

    private static void CopyDirectory(string sourceDir, string destDir, bool overwrite)
    {
        // Create destination directory if it doesn't exist
        Directory.CreateDirectory(destDir);

        // Copy all files
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite);
        }

        // Copy all subdirectories
        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
            CopyDirectory(subDir, destSubDir, overwrite);
        }
    }
}
