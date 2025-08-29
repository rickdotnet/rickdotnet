using DasMonitor;
using Microsoft.Extensions.Hosting;

var host = Host
    .CreateApplicationBuilder()
    .ConfigureHost();

host.Run();
//await DasDemo.HostDemo(host);
