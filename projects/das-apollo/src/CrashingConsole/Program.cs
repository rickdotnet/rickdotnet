using Apollo.Extensions.Microsoft.Hosting;
using Apollo.Providers.NATS;
using DasMonitor.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client.Core;

// this console app simulates an online service
// that crashes and signals daskeyboard
var services = new ServiceCollection();
services.AddHttpClient<DasSignalClient>();
services.AddSingleton<DasPublisher>();
services.AddApollo(ab => ab
    .PublishOnly()
    .AddNatsProvider(opts => opts with { Url = "nats://localhost:4222" })
);

var serviceProvider = services.BuildServiceProvider();
var publisher = serviceProvider.GetRequiredService<DasPublisher>();

// we're up and running
Console.WriteLine("We're up and running!");
await publisher.PublishSignal(
    new AddSignalCommand
    {
        ProductId = "DK5QPID",
        ZoneId = "KEY_1",
        HexColor = "#FFF",
        Effect = DasEffect.SetColor,
        Title = "Online",
        Message = "We'll never crash!",
    }
);

// still holding
await Task.Delay(TimeSpan.FromSeconds(10));

// uh oh, we're crashing
var currentColor = Console.ForegroundColor;
Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine("We crashed!");
Console.ForegroundColor = currentColor;

await publisher.PublishSignal(
    new AddSignalCommand
    {
        ProductId = "DK5QPID",
        ZoneId = "KEY_1",
        HexColor = "#F00",
        Effect = DasEffect.Blink,
        Title = "ERROR",
        Message = "Just kidding!",
    }
);

Console.Write("Press any key to exit");
Console.ReadKey();

await publisher.ClearSignal(new ClearSignalCommand
{
    ProductId = "DK5QPID",
    ZoneId = "KEY_1",
});