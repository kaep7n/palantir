using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;
using Proto.Cluster.PubSub;

namespace Palantir;

public class Room : RoomGrainBase
{
    private readonly List<Device> devices = new List<Device>();
    private readonly IContext context;
    private readonly ClusterIdentity clusterIdentity;
    private ILogger<Room> logger;

    public Room(
        IContext context,
        ClusterIdentity clusterIdentity,
        ILogger<Room> logger
        ) : base(context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.clusterIdentity = clusterIdentity ?? throw new ArgumentNullException(nameof(clusterIdentity));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override Task<RoomInitialzed> Initialize(IntializeRoom request)
    {
        foreach (var deviceConfig in request.Definition.Devices)
        {
            var device = deviceConfig.Type switch
            {
                "Shutter" => new Shutter { Name = deviceConfig.Name },
                "Dimmer" => new Dimmer { Name = deviceConfig.Name },
                "Switch" => new Switch { Name = deviceConfig.Name },
                "Lock" => new Lock { Name = deviceConfig.Name },
                "Thermostat" => new Thermostat { Name = deviceConfig.Name } as Device,
                _ => throw new ArgumentOutOfRangeException(nameof(request), $"Unexpected device type '{deviceConfig.Type}'")
            };

            this.devices.Add(device);
        }

        return Task.FromResult(new RoomInitialzed());
    }

    public override Task<RoomJoined> Join(JoinRoom request)
    {
        this.logger.LogInformation($"device {request.DeviceId} {request.ChannelId} joined Room {this.clusterIdentity}");

        this.Cluster.Subscribe($"values/{request.DeviceId}/{request.ChannelId}", context =>
        {
            if (context.Message is ValueChanged vc)
            {
                this.logger.LogInformation($"{this.clusterIdentity}: value changed {vc}");
            }

            return Task.CompletedTask;
        });

        return Task.FromResult(new RoomJoined());
    }

}
