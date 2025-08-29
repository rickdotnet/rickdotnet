namespace Library.Shell;

public class ShellCommandResult
{
    public int ExitCode { get; }
    public bool IsSuccess => ExitCode == 0;
    public string StandardOutput { get; }
    public string StandardError { get; }
    public DateTimeOffset? StartTime { get; }
    public DateTimeOffset? StopTime { get; }

    public TimeSpan ExecutionTime =>
        StopTime.HasValue && StartTime.HasValue
            ? StopTime.Value - StartTime.Value
            : TimeSpan.Zero;

    public ShellCommandResult(
        int exitCode,
        string stdOut,
        string stdErr,
        DateTimeOffset? startTime = null,
        DateTimeOffset? stopTime = null)
    {
        ExitCode = exitCode;
        StandardOutput = stdOut;
        StandardError = stdErr;
        StartTime = startTime;
        StopTime = stopTime;
    }
}