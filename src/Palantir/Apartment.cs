using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;

namespace Palantir;

public class Apartment : ApartmentGrainBase
{
    private readonly IContext context;
    private readonly ClusterIdentity clusterIdentity;
    private ILogger<Apartment> logger;

    private readonly List<RoomGrainClient> rooms = new List<RoomGrainClient>();

    public Apartment(
        IContext context,
        ClusterIdentity clusterIdentity,
        ILogger<Apartment> logger) : base(context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.clusterIdentity = clusterIdentity ?? throw new ArgumentNullException(nameof(clusterIdentity));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<ApartmentInitialized> Initialize(InitializeApartment request)
    {
        foreach (var roomDefinition in request.Definition.Rooms)
        {
            var room = this.context.Cluster().GetRoomGrain(roomDefinition.Name);

            await room.Initialize(new IntializeRoom { Definition = roomDefinition }, this.context.CancellationToken)
                .ConfigureAwait(false);

            this.rooms.Add(room);
        }

        return new ApartmentInitialized();
    }

}
