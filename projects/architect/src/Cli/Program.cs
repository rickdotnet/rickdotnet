using Architect.Cli;
using Architect.Cli.Commands;
using Microsoft.Extensions.Hosting;
using ConsoleAppFramework;
using Library.Shell.Wrappers;
using Microsoft.Extensions.DependencyInjection;

// var updateResult = await Paru.UpdateAll();

var result = await Paru.Exists("fastfetch");
Console.WriteLine($"Fastfetch exists: {result}");
if (!result)
    await Pacman.Install("fastfetch");


//var result = ShellCommand.Run("echo", "\"Hello World\"");
//var result2 = ShellCommand.Run("paru", "-Syu --noconfirm");
// var sudoResult = ShellCommand.Run("sudo", "-v"); // cache the password
//
// var command = ShellCommand.Build("echo", "\"Hello World\"")
//     .WithOutputSink(Console.WriteLine)
//     .WithSudo() // password will be cached
//     .Create();
//
// var result2 = await command.ExecuteAsync();

return;

using var host = Host.CreateDefaultBuilder().Build();
using var scope = host.Services.CreateScope();
ConsoleApp.ServiceProvider = scope.ServiceProvider;

var app = ConsoleApp.Create();
app.Add<Application>(); // base application
app.Add<Install>("install"); // tools

app
    .ConfigureLogging(Startup.Logging)
    .ConfigureDefaultConfiguration(Startup.Configuration)
    .ConfigureServices(Startup.Services)
    .Run(args);
