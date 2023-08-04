using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;

namespace Palantir;

public class Apartment(IContext context, ClusterIdentity clusterIdentity, ILogger<Apartment> logger) : ApartmentGrainBase(context)
{
    private readonly IContext context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ClusterIdentity clusterIdentity = clusterIdentity ?? throw new ArgumentNullException(nameof(clusterIdentity));
    private readonly ILogger<Apartment> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly List<RoomGrainClient> rooms = new();

    public override async Task<ApartmentInitialized> Initialize(InitializeApartment request)
    {
        foreach (var roomDefinition in request.Definition.Rooms)
        {
            var room = this.context.Cluster().GetRoomGrain(roomDefinition.Id);

            await room.Initialize(new IntializeRoom { Definition = roomDefinition }, this.context.CancellationToken)
                .ConfigureAwait(false);

            this.rooms.Add(room);
        }

        return new ApartmentInitialized();
    }

}
