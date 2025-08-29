namespace Library.Shell;

public static class Shell
{
    public static ShellCommandResult Run(
        string command,
        string arguments = "",
        SudoMode sudoMode = SudoMode.None,
        Func<Task<string>>? passwordProvider = null)
    {
        var builder = ShellCommand.Build(command, arguments)
            .WithOutputSink(Console.WriteLine);

        if (sudoMode == SudoMode.Explicit && passwordProvider != null)
            builder.WithSudo(passwordProvider);

        return builder.Create().Execute();
    }

    public static async Task<ShellCommandResult> RunAsync(
        string command,
        string arguments = "",
        SudoMode sudoMode = SudoMode.None,
        Func<Task<string>>? passwordProvider = null,
        CancellationToken cancellationToken = default)
    {
        var builder = ShellCommand.Build(command, arguments)
            .WithOutputSink(Console.WriteLine);

        if (sudoMode == SudoMode.Explicit && passwordProvider != null)
            builder.WithSudo(passwordProvider);

        return await builder.Create().ExecuteAsync(cancellationToken);
    }
}
