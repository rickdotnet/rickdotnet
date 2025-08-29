using Aether;
using Aether.Abstractions.Messaging;
using Dusty.Cheese;
using Dusty.Shared;
using Dusty.Shared.Hosting;
using Dusty.Shared.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var builder = Host.CreateApplicationBuilder(args);
builder.AddLogging();
AppConfig config = builder.AddConfig();

builder.Services.AddSingleton<ExecuteEndpoint>();
builder.Services.AddMessaging(config, hub => hub.AddEndpoint<ExecuteEndpoint>(ExecuteEndpoint.Config));

var host = builder.Build();
var provider = host.Services;
var aether = provider.GetRequiredService<AetherClient>();

await Task.Delay(3000);
Foreman.Aether = aether; // demo cheese
await aether.Messaging.Send(AetherMessage.For("dusty.execute", "Hello, cheese!"));
host.Run();
