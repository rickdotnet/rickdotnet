namespace Library.Shell.Wrappers;

public class Pacman
{
    public static Task <ShellCommandResult> Install(string package) 
        => Shell.RunAsync("sudo", $"pacman -S {package} --noconfirm");
    
    public static Task <ShellCommandResult> Remove(string package) 
        => Shell.RunAsync("sudo", $"pacman -R {package} --noconfirm");

    public static async Task<bool> Exists(string package)
    {
        var result = await Shell.RunAsync("pacman", $"-Q {package}");
        return result.ExitCode switch
        {
            0 => true,
            1 => false,
            _ => throw new Exception($"Error checking if package {package} exists: {result.StandardError}")
        };
    }
    
}
