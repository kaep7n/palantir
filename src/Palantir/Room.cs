using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Proto;
using Proto.Cluster;
using Proto.Cluster.PubSub;

namespace Palantir;

public class Room : RoomGrainBase
{
    private readonly List<Device> devices = new List<Device>();
    private readonly IContext context;
    private readonly ClusterIdentity clusterIdentity;
    private readonly IOptionsMonitor<RoomConfig> optionsMonitor;
    private ILogger<Room> logger;

    public Room(IContext context, ClusterIdentity clusterIdentity, IOptionsMonitor<RoomConfig> optionsMonitor, ILogger<Room> logger) : base(context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.clusterIdentity = clusterIdentity ?? throw new ArgumentNullException(nameof(clusterIdentity));
        this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override Task OnStarted()
    {
        var config = this.optionsMonitor.Get(this.clusterIdentity.Identity);

        if (config?.Devices is not null)
        {
            foreach (var deviceConfig in config.Devices)
            {
                var device = deviceConfig.Type switch
                {
                    "Shutter" => new Shutter { Name = deviceConfig.Name } as Device,
                    "Dimmer" => new Dimmer { Name = deviceConfig.Name } as Device,
                    "Switch" => new Switch { Name = deviceConfig.Name } as Device,
                    "Thermostat" => new Thermostat { Name = deviceConfig.Name } as Device,
                    _ => throw new ArgumentOutOfRangeException("Unexpected device type")
                };

                this.devices.Add(device);
            }
        }

        return base.OnStarted();
    }

    public override Task OnReceive()
    {
        return base.OnReceive();
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
