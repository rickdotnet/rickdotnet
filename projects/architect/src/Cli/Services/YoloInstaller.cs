using SimpleExec;

namespace Architect.Cli.Services;

public class YoloInstaller
{
    public static async Task UserInstall()
    {
        return;
        await Command.RunAsync("bash", "-c \"git clone https://aur.archlinux.org/paru.git /tmp/paru && cd /tmp/paru && makepkg -si --noconfirm\"");
        await Command.RunAsync("paru", "-S  grimblast-git oh-my-posh-bin --noconfirm");

        Console.WriteLine("Copying configs to ~/.config...");

        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var userConfigDir = Path.Combine(homeDir, ".config");
        var tempConfigDir = Path.Combine(homeDir, "dotfiles", "config");

        Console.WriteLine($"Using {tempConfigDir} as source.");
        ConfigCopier.Copy(tempConfigDir, userConfigDir);

        var sourceIdeavimrc = Path.Combine(userConfigDir, "vim", ".ideavimrc");
        var targetIdeavimrc = Path.Combine(homeDir, ".ideavimrc");
        CreateSymlink(sourceIdeavimrc, targetIdeavimrc);

        var sourceBashrc = Path.Combine(userConfigDir, "bash", ".bashrc");
        var targetBashrc = Path.Combine(homeDir, ".bashrc");
        CreateSymlink(sourceBashrc, targetBashrc);
    }

    public static async Task SudoInstall()
    {
        Console.WriteLine("Starting YOLO install");
        return;

        await Command.RunAsync("pacman", "-S --needed base-devel --noconfirm");
        await Command.RunAsync("pacman", "-S libnotify hyprpicker wl-clipboard grim slurp rofi-wayland noto-fonts-emoji ttf-jetbrains-mono-nerd waybar --noconfirm");
        await Command.RunAsync("pacman", "-S ghostty yazi ffmpeg p7zip jq poppler fd ripgrep fzf zoxide imagemagick --noconfirm");
        await Command.RunAsync("pacman", "-S dotnet-sdk --noconfirm");
        await Command.RunAsync("pacman", "-S gnome-keyring libsecret --noconfirm");
        await Command.RunAsync("pacman", "-S discord flatpak --noconfirm");
    }

    private static void CreateSymlink(string sourcePath, string targetPath)
    {
        if (!File.Exists(sourcePath))
        {
            Console.WriteLine($"Source file {sourcePath} does not exist. Skipping symlink creation.");
            return;
        }

        if (File.Exists(targetPath))
            File.Delete(targetPath);

        if (Directory.Exists(targetPath))
            Directory.Delete(targetPath, true);

        File.CreateSymbolicLink(targetPath, sourcePath);
        Console.WriteLine($"Created symlink: {targetPath} -> {sourcePath}");
    }

    public static class ConfigCopier
    {
        public static void Copy(string sourceDir, string destDir)
        {
            Console.WriteLine($"Copying configs from {sourceDir} to {destDir}...");

            if (!Directory.Exists(sourceDir))
            {
                Console.WriteLine($"Source directory {sourceDir} does not exist. Skipping config copy.");
                return;
            }

            CopyDirectory(sourceDir, destDir, true);
            Console.WriteLine($"Configs copied to {destDir}");
        }

        private static void CopyDirectory(string sourceDir, string destDir, bool overwrite)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite);
            }

            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir, overwrite);
            }
        }
    }
}
