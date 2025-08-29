using System.Diagnostics;
using System.Text;

namespace Library.Shell;

public class ShellCommand
{
    private readonly string command;
    private readonly string arguments;
    private readonly SudoMode sudoMode;
    private readonly Func<Task<string>> passwordProvider;
    private readonly Dictionary<string, string> environment;
    private readonly string workingDirectory;
    private readonly TimeSpan? timeout;
    private readonly bool dryRun;
    private readonly Action<string>? stdOutSink;
    private readonly Action<string>? stdErrSink;
    public bool IsRunning { get; private set; }

    public ShellCommand(
        string command,
        string arguments,
        SudoMode sudoMode,
        Func<Task<string>> passwordProvider,
        Dictionary<string, string> environment,
        string workingDirectory,
        TimeSpan? timeout,
        bool dryRun,
        Action<string>? stdOutSink,
        Action<string>? stdErrSink)
    {
        this.command = command;
        this.arguments = arguments;
        this.sudoMode = sudoMode;
        this.passwordProvider = passwordProvider;
        this.environment = environment;
        this.workingDirectory = workingDirectory;
        this.timeout = timeout;
        this.dryRun = dryRun;
        this.stdOutSink = stdOutSink;
        this.stdErrSink = stdErrSink;
    }

    public static ShellCommand Create(string command, string arguments = "", bool outputToConsole = true)
    {
        var builder = Build(command, arguments);

        if (outputToConsole)
            builder.WithOutputSink(Console.WriteLine);

        return builder.Create();
    }

    public static ShellCommandBuilder Build(string command, string arguments = "")
        => ShellCommandBuilder.For(command, arguments);

    public ShellCommandResult Execute()
        => ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

    public async Task<ShellCommandResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var explicitSudo = sudoMode == SudoMode.Explicit;
        if (dryRun)
        {
            var cmdLine = explicitSudo ? $"sudo {command} {arguments}" : $"{command} {arguments}";
            stdOutSink?.Invoke($"[DryRun] Would execute: {cmdLine}");
            return new ShellCommandResult(0, $"[DryRun] {cmdLine}", "", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        }

        IsRunning = true;
        var startTime = DateTimeOffset.UtcNow;
        var stdOut = new StringBuilder();
        var stdErr = new StringBuilder();

        if (sudoMode == SudoMode.None)
            return await TryExecuteAsync();

        // TODO: figure out how we want to handle sudo errors
        var result = await TryExecuteAsync();
        return result;


        // if (result.ExitCode != 0 && IsPermissionError(result.StandardError))
        //     return result;
        //
        // if(sudoMode == SudoMode.Auto)
        // {
        //     stdOutSink?.Invoke("Permission error detected, retrying with sudo...");
        //     return await TryExecuteAsync(true);
        // }


        async Task<ShellCommandResult> TryExecuteAsync(bool forceSudo = false)
        {
            try
            {
                var useSudo = explicitSudo || forceSudo;
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = useSudo ? "sudo" : command,
                    Arguments = useSudo ? $"-S {command} {arguments}" : arguments,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardInput = sudoMode != SudoMode.None,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                foreach (var env in environment)
                    process.StartInfo.Environment[env.Key] = env.Value;

                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data == null)
                        return;

                    stdOut.AppendLine(e.Data);
                    stdOutSink?.Invoke(e.Data);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data == null)
                        return;

                    stdErr.AppendLine(e.Data);
                    stdErrSink?.Invoke(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (useSudo)
                {
                    var password = await passwordProvider.Invoke();
                    if (string.IsNullOrEmpty(password))
                    {
                        // TODO: figure out if I care to handle this
                        //              This essentially warns in case the process hangs waiting for
                        //              password input. Shouldn't happen if the password is cached.
                        //              but wouldn't be very user friendly if it does. I think this would
                        //              require me to wrap the stream to detect the password prompt.
                        //              Not interested in doing that right now.
                        stdErrSink?.Invoke("Password is required for sudo command but not provided.");
                    }
                    if (!string.IsNullOrEmpty(password))
                    {
                        await process.StandardInput.WriteLineAsync(password);
                        await process.StandardInput.FlushAsync(cancellationToken);
                    }
                }

                if (timeout.HasValue)
                {
                    if (await process.WaitForExitAsync(timeout.Value, cancellationToken))
                        return new ShellCommandResult(process.ExitCode, stdOut.ToString().Trim(), stdErr.ToString().Trim(), startTime, DateTimeOffset.UtcNow);

                    process.Kill();
                    stdErr.AppendLine($"Process timed out after {timeout.Value.TotalSeconds} seconds");
                    return new ShellCommandResult(-1, stdOut.ToString().Trim(), stdErr.ToString().Trim(), startTime, DateTimeOffset.UtcNow);
                }

                await process.WaitForExitAsync(cancellationToken);
                return new ShellCommandResult(process.ExitCode, stdOut.ToString().Trim(), stdErr.ToString().Trim(), startTime, DateTimeOffset.UtcNow);
            }
            finally
            {
                IsRunning = false;
            }
        }

        bool IsPermissionError(string errorLine)
        {
            if (string.IsNullOrEmpty(errorLine))
                return false;

            // todo: check on this
            return errorLine.Contains("permission denied", StringComparison.OrdinalIgnoreCase) ||
                   errorLine.Contains("sudo:", StringComparison.OrdinalIgnoreCase);
        }
    }
}

public static class ProcessExtensions
{
    public static async Task<bool> WaitForExitAsync(this Process process, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var task = process.WaitForExitAsync(cts.Token);

        var completed = task == await Task.WhenAny(task, Task.Delay(timeout, cts.Token));
        if (!completed)
            await cts.CancelAsync();

        return completed;
    }
}
