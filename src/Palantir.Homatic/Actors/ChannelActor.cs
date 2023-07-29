using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;
using Proto.Cluster.PubSub;
using Proto.DependencyInjection;

namespace Palantir.Homatic.Actors;

public class ChannelActor : IActor
{
    private readonly PID apiPool;
    private readonly string deviceId;
    private readonly string id;
    private readonly ILogger<ChannelActor> logger;

    private readonly Dictionary<string, PID> parameters = new();

    public ChannelActor(
        PID apiPool,
        string deviceId,
        string id,
        ILogger<ChannelActor> logger)
    {
        this.apiPool = apiPool ?? throw new ArgumentNullException(nameof(apiPool));
        this.deviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
        this.id = id ?? throw new ArgumentNullException(nameof(id));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ReceiveAsync(IContext context)
    {
        if (context.Message is Started)
        {
            this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);

            context.Request(this.apiPool, new GetChannel(this.deviceId, this.id), context.Self);
        }
        else if (context.Message is GetChannelResult result)
        {
            var homaticRooms = result.Channel.Links.Where(l => l.Rel == "room");

            foreach (var homaticRoom in homaticRooms)
            {
                //var homaticRoomId = homaticRoom.Href.Replace("/room/", string.Empty);

                //var room = context.Cluster().GetRoomGrain(Rooms.GetClusterIdentity(homaticRoomId));

                //var joinRoom = new JoinRoom(this.deviceId, this.id, homaticRoom.Href.Replace("/room/", string.Empty));
                //var roomJoined = await room.Join(joinRoom, CancellationToken.None);
            }

            foreach (var link in result.Channel.Links)
            {
                if (link.Href == ".." || link.Rel != "parameter")
                    continue;

                var props = context.System.DI().PropsFor<ParameterActor>(this.apiPool, this.deviceId, this.id, link.Href);
                var pid = context.Spawn(props);

                this.parameters.Add(link.Href, pid);
            }
        }
        else if (context.Message is ParameterValueChanged pvc)
        {
            var parameter = this.parameters[pvc.Parameter];

            context.Forward(parameter);

            var publisher = context.Cluster().Publisher();

            await publisher.Publish($"values/{pvc.Device}/{pvc.Channel}", new ValueChanged { Type = "test", Value = pvc.Value.ToString() });
        }
        else if (context.Message is Stopped)
        {
            this.logger.LogDebug("{type} ({pid}) has stopped", this.GetType(), context.Self);
        }
    }
}
