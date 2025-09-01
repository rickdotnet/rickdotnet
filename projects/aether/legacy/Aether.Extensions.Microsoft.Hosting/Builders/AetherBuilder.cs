using Aether.Abstractions.Hosting;
using Aether.Abstractions.Messaging;
using Aether.Abstractions.Storage;
using Aether.Extensions.Microsoft.Hosting.Messaging;
using Aether.Messaging;
using Aether.Providers.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Aether.Extensions.Microsoft.Hosting.Builders;

public class AetherBuilder : IAetherBuilder
{
    private readonly IServiceCollection services;
    private readonly ServiceCollection internalServices = [];

    public IMessagingBuilder Messaging { get; }
    public IStorageBuilder Storage { get; }

    public AetherBuilder(IServiceCollection services)
    {
        this.services = services;
        Messaging = new MessagingBuilder(this);
        Storage = new StorageBuilder(this);

        services.AddSingleton<MemoryHub>();
        services.AddSingleton<MemoryStore>();
    }

    internal void Build()
    {
        // register our services with the provided IServiceCollection
        // the reason we're not directly adding the services to the provided IServiceCollection is
        //   1) to allow for additional services to be added by the consumer without affecting the hosted services
        //   2) to provide control over which services are added and in what order
        // for now, we're just adding all the services to the provided IServiceCollection
        foreach (var service in internalServices)
            services.Add(service);

        services.AddSingleton<AetherClient>(serviceProvider =>
        {
            var defaultHub = (IMessageHub)serviceProvider.GetRequiredService(Messaging.DefaultHubType);
            var storage = (IStore)serviceProvider.GetRequiredService(Storage.DefaultStoreType);

            var defaultAetherHub = new AetherHub(defaultHub);
            var client = new AetherClient(defaultAetherHub, storage);

            // foreach registration, set the hubs
            var hubRegistrations = ((MessagingBuilder)Messaging).HubRegistrations;
            foreach (var registration in hubRegistrations)
            {
                if (registration.HubName == IDefaultMessageHub.DefaultHubKey)
                {
                    RegisterHandlers(defaultAetherHub, registration.EndpointRegistrations, serviceProvider);
                }
                else
                {
                    var hubType = registration.HubType;
                    var hub = (IMessageHub)serviceProvider.GetRequiredService(hubType);

                    var aetherHub = AetherHub.For(hub);
                    client.Messaging.SetHub(registration.HubName, aetherHub);

                    RegisterHandlers(aetherHub, registration.EndpointRegistrations, serviceProvider);
                }
            }

            var storageRegistrations = ((StorageBuilder)Storage).StoreRegistrations;
            foreach (var storeRegistration in storageRegistrations)
            {
                if (storeRegistration.StoreName == IDefaultStore.DefaultStoreName)
                    continue;

                client.Storage.SetStore(
                    storeRegistration.StoreName,
                    (IStore)serviceProvider.GetRequiredService(storeRegistration.StoreType!)
                );
            }

            return client;
        });

        services.AddSingleton<IAetherClient>(p => p.GetRequiredService<AetherClient>());
        //services.AddHostedService<AetherBackgroundService>();
    }

    private static void RegisterHandlers(AetherHub hub, IReadOnlyList<EndpointRegistration> registrations, IServiceProvider provider)
    {
        foreach (var registration in registrations)
        {
            if (registration.IsHandler)
            {
                var handler = new AetherHandler(registration.Handler!);
                hub.AddHandler(
                    registration.Config,
                    registration.HandlerConfig,
                    handler.Handle,
                    CancellationToken.None // cancellation will be handled in DefaultMessageHub
                );
            }
            else
            {
                var endpointProvider = new DefaultEndpointProvider(provider);
                var endpointHandler = new AetherHandler(registration.EndpointType!, endpointProvider);
                hub.AddHandler(
                    registration.Config,
                    registration.HandlerConfig,
                    endpointHandler.Handle,
                    CancellationToken.None // cancellation will be handled in DefaultMessageHub
                );
            }
        }
    }

    public void RegisterServices(Action<IServiceCollection> registerAction)
        => registerAction(internalServices);
}
