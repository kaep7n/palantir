using Proto;
using Proto.DependencyInjection;

namespace Palantir
{
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
                try
                {
                    logger.LogDebug("{type} ({pid}) has started", GetType(), context.Self);

                    var channel = await homaticClient.GetChannelAsync(deviceId, id);

                    foreach (var link in channel.Links)
                    {
                        if (link.Href == ".." || link.Rel != "parameter")
                            continue;

                        var props = context.System.DI().PropsFor<HomaticDeviceChannelParameterActor>(deviceId, id, link.Href);
                        var pid = context.Spawn(props);

                        parameters.Add(link.Href, pid);
                    }

                    var rooms = channel.Links.Where(l => l.Rel == "room");

                    foreach (var room in rooms)
                    {
                        var join = new JoinRoom(deviceId, id, room.Href.Replace("/room/", string.Empty));
                        context.Send(context.Parent, join);
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "HomaticDeviceChannelActor");
                }
            }
            if (context.Message is ParameterValueChanged pvc)
            {
                var parameter = this.parameters[pvc.Parameter];

                context.Forward(parameter);
            }
            if (context.Message is Stopped)
            {
                logger.LogDebug("{type} ({pid}) has started", GetType(), context.Self);
            }
        }
    }
}
