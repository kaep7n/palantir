using Microsoft.Extensions.Logging;
using Palantir.Homatic.Http;
using Proto;
using Proto.Cluster;
using Proto.Cluster.PubSub;
using Proto.DependencyInjection;

namespace Palantir.Homatic.Actors;

public class HomaticDeviceChannelActor : IActor
{
    private readonly string deviceId;
    private readonly string id;
    private readonly HomaticHttpClient homaticClient;
    private readonly ILogger<HomaticDeviceChannelActor> logger;

    private readonly Dictionary<string, PID> parameters = new Dictionary<string, PID>();

    public HomaticDeviceChannelActor(
        string deviceId,
        string id,
        HomaticHttpClient homaticClient,
        ILogger<HomaticDeviceChannelActor> logger)
    {
        this.deviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
        this.id = id ?? throw new ArgumentNullException(nameof(id));
        this.homaticClient = homaticClient ?? throw new ArgumentNullException(nameof(homaticClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ReceiveAsync(IContext context)
    {
        if (context.Message is Started)
        {
            this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);

            var channel = await this.homaticClient.GetChannelAsync(this.deviceId, this.id);

            var homaticRooms = channel.Links.Where(l => l.Rel == "room");

            foreach (var homaticRoom in homaticRooms)
            {
                var homaticRoomId = homaticRoom.Href.Replace("/room/", string.Empty);

                var room = context.Cluster().GetRoomGrain(Rooms.GetClusterIdentity(homaticRoomId));

                var joinRoom = new JoinRoom(this.deviceId, this.id, homaticRoom.Href.Replace("/room/", string.Empty));
                var roomJoined = await room.Join(joinRoom, CancellationToken.None);
            }

            foreach (var link in channel.Links)
            {
                if (link.Href == ".." || link.Rel != "parameter")
                    continue;

                var props = context.System.DI().PropsFor<HomaticDeviceChannelParameterActor>(this.deviceId, this.id, link.Href);
                var pid = context.Spawn(props);

                this.parameters.Add(link.Href, pid);
            }
        }
        if (context.Message is ParameterValueChanged pvc)
        {
            var parameter = this.parameters[pvc.Parameter];

            context.Forward(parameter);

            var publisher = context.Cluster().Publisher();

            await publisher.Publish($"values/{pvc.Device}/{pvc.Channel}", new ValueChanged { Type = "test", Value = pvc.Value.ToString() });
        }
        if (context.Message is Stopped)
        {
            this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);
        }
    }
}
