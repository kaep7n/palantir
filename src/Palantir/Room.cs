using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;

namespace Palantir;

public class Room(IContext context, ClusterIdentity clusterIdentity, ILogger<Room> logger) : RoomGrainBase(context)
{
    private readonly IContext context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ClusterIdentity clusterIdentity = clusterIdentity ?? throw new ArgumentNullException(nameof(clusterIdentity));
    private readonly ILogger<Room> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly Dictionary<string, PID> devices = new();

    private RoomDefinition roomDefinition = new() { Id = clusterIdentity.Identity };

    private double currentTemperature;
    private double setTemperature;

    public override Task<RoomInitialzed> Initialize(IntializeRoom request)
    {
        this.roomDefinition = request.Definition;

        return Task.FromResult(new RoomInitialzed());
    }

    public override Task<RoomJoined> Join(JoinRoom request)
    {
        this.logger.LogInformation("{device} joined Room {room}", request.Id, this.roomDefinition?.Id);

        this.devices.Add(request.Id, PID.FromAddress(request.Sender.Address, request.Sender.Id));

        return Task.FromResult(new RoomJoined { Definition = this.roomDefinition });
    }

    public override Task OnTemperatureChanged(TemperatureChanged request)
    {
        this.currentTemperature = request.Value;

        this.logger.LogInformation("Temperature changed {@request}", request);

        return Task.CompletedTask;
    }

    public override Task OnSetTemperatureChanged(SetTemperatureChanged request)
    {
        this.setTemperature = request.Value;

        this.logger.LogInformation("Set Temperature changed {@request}", request);

        return Task.CompletedTask;
    }
}
