using Microsoft.Extensions.Options;
using Proto;
using Proto.Cluster;

namespace Palantir.Sys;

public class ActorSystemService : IHostedService
{
    private readonly ActorSystem actorSystem;
    private readonly IOptionsMonitor<ApartmentOption> optionsMonitor;
    private readonly ILogger<ActorSystemService> logger;

    public ActorSystemService(
        ActorSystem actorSystem,
        IOptionsMonitor<ApartmentOption> optionsMonitor,
        ILogger<ActorSystemService> logger)
    {
        this.actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
        this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await this.actorSystem.Cluster()
             .StartMemberAsync();

        var apartment = this.actorSystem.Cluster().GetApartmentGrain("Apartment");

        var definition = new ApartmentDefinition();

        foreach (var room in this.optionsMonitor.CurrentValue.Rooms)
        {
            var roomDefinition = new RoomDefinition()
            {
                Name = room.Name
            };

            foreach (var device in room.Devices)
            {
                var deviceDefinition = new DeviceDefinition { Type = device.Type, Name = device.Name };

                roomDefinition.Devices.Add(deviceDefinition);
            }

            definition.Rooms.Add(roomDefinition);
        }

        _ = await apartment.Initialize(new InitializeApartment { Definition = definition }, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await this.actorSystem
            .Cluster()
            .ShutdownAsync()
            .ConfigureAwait(false);
    }
}