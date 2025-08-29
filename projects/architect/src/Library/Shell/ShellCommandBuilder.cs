namespace Library.Shell;

public class ShellCommandBuilder
{
    private string command = string.Empty;
    private string arguments = string.Empty;
    private SudoMode sudoMode = SudoMode.None;
    private Func<Task<string>> passwordProvider = () => Task.FromResult(string.Empty);
    private readonly Dictionary<string, string> environment = new();
    private string workingDirectory = Directory.GetCurrentDirectory();
    private TimeSpan? timeout;
    private bool dryRun;
    private Action<string>? stdOutSink;
    private Action<string>? stdErrSink;

    public static ShellCommandBuilder New() => new();

    public static ShellCommandBuilder For(string command, string arguments = "")
    {
        return new ShellCommandBuilder()
            .WithCommand(command)
            .WithArguments(arguments);
    }

    public ShellCommandBuilder WithCommand(string command)
    {
        this.command = command;
        return this;
    }

    public ShellCommandBuilder WithArguments(string arguments)
    {
        this.arguments = arguments;
        return this;
    }
    


    /// <summary>
    /// Typically used after a sudo -v command to cache the password
    /// </summary>
    public ShellCommandBuilder WithSudo()
    {
        sudoMode = SudoMode.Explicit;
        passwordProvider = () => Task.FromResult(string.Empty);
        return this;
    }
    
    /// <summary>
    /// Used to set the password for sudo commands.
    /// </summary>
    /// <param name="sudoProvider">Function that returns the password for sudo commands.</param>
    public ShellCommandBuilder WithSudo(Func<string> sudoProvider) 
        => WithSudo(sudoProvider());
    
    /// <summary>
    /// Used to set the password for sudo commands.
    /// </summary>
    public ShellCommandBuilder WithSudo(string password)
    {
        sudoMode = SudoMode.Explicit;
        passwordProvider = () => Task.FromResult(password);
        return this;
    }

    /// <summary>
    /// Used to set the password for sudo commands.
    /// </summary>
    /// <param name="sudoProvider">Function that returns the password for sudo commands.</param>
    public ShellCommandBuilder WithSudo(Func<Task<string>> sudoProvider)
    {
        sudoMode = SudoMode.Explicit;
            passwordProvider = sudoProvider;
        
        return this;
    }

    public ShellCommandBuilder WithEnvironment(string key, string value)
    {
        environment[key] = value;
        return this;
    }

    public ShellCommandBuilder WithWorkingDirectory(string directory)
    {
        workingDirectory = directory;
        return this;
    }

    public ShellCommandBuilder WithTimeout(TimeSpan timeout)
    {
        this.timeout = timeout;
        return this;
    }

    public ShellCommandBuilder DryRun(bool dryRun = true)
    {
        this.dryRun = dryRun;
        return this;
    }

    public ShellCommandBuilder WithOutputSink(Action<string>? stdOutSink = null, bool includeStdErr = false)
    {
        this.stdOutSink = stdOutSink;
        if (includeStdErr)
            this.stdErrSink = stdOutSink;
        return this;
    }
    
    public ShellCommand Create()
    {
        if (string.IsNullOrEmpty(command))
            throw new InvalidOperationException("Command cannot be empty.");

        return new ShellCommand(
            command,
            arguments,
            sudoMode,
            passwordProvider,
            new Dictionary<string, string>(environment), 
            workingDirectory,
            timeout,
            dryRun,
            stdOutSink,
            stdErrSink);
    }
}
