using Microsoft.Extensions.Options;
using Proto;
using Proto.Cluster;

namespace Palantir.Sys;

public class ActorSystemService(ActorSystem actorSystem, IOptionsMonitor<ApartmentOptions> optionsMonitor, ILogger<ActorSystemService> logger) : IHostedService
{
    private readonly ActorSystem actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
    private readonly IOptionsMonitor<ApartmentOptions> optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
    private readonly ILogger<ActorSystemService> logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
                Id = room.Id,
                Name = room.Name,
                Type = room.Type
            };

            definition.Rooms.Add(roomDefinition);
        }

        await apartment.Initialize(new InitializeApartment { Definition = definition }, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await this.actorSystem
            .Cluster()
            .ShutdownAsync()
            .ConfigureAwait(false);
    }
}