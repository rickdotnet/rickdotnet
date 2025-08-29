namespace Library.Shell.Wrappers;

public class Paru
{
    public static Task<ShellCommandResult> UpdateAll()
        => Shell.RunAsync("paru", "-Syu --noconfirm");
    
    public static Task <ShellCommandResult> Install(string package) 
        => Shell.RunAsync("paru", $"-S  {package} --needed --noconfirm");
    
    public static Task <ShellCommandResult> Remove(string package) 
        => Shell.RunAsync("paru", $"-R {package} --noconfirm");

    public static async Task<bool> Exists(string package)
    {
        var result = await Shell.RunAsync("paru", $"-Q {package}");
        return result.ExitCode switch
        {
            0 => true,
            1 => false,
            _ => throw new Exception($"Error checking if package {package} exists: {result.StandardError}")
        };
    }
}
