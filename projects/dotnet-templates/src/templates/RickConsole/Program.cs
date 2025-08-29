using Microsoft.Extensions.Hosting;
using RickConsole;

var host = Host.CreateApplicationBuilder(args).ConfigureHost();
host.Run();
