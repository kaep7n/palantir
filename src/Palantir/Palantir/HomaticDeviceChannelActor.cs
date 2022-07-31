using Proto;
using Proto.DependencyInjection;

namespace Palantir
{
    public class HomaticDeviceChannelActor : IActor
    {
        private readonly string deviceId;
        private readonly string id;
        private readonly HomaticHttpClient homaticClient;
        private readonly ILogger<HomaticActor> logger;

        private readonly Dictionary<string, PID> parameters = new Dictionary<string, PID>();

        public HomaticDeviceChannelActor(
            string deviceId,
            string id,
            HomaticHttpClient homaticClient,
            ILogger<HomaticActor> logger)
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
                logger.LogInformation("{type} ({pid}) has started", GetType(), context.Self);

                var channel = await homaticClient.GetChannelAsync(deviceId, id);

                foreach (var link in channel.Links)
                {
                    if (link.Href == ".." || link.Rel != "parameter")
                        continue;

                    var props = context.System.DI().PropsFor<HomaticDeviceChannelParameterActor>(deviceId, id, link.Href);
                    var pid = context.Spawn(props);

                    parameters.Add(link.Href, pid);
                }
            }
            if (context.Message is Stopped)
            {
                logger.LogInformation("{type} ({pid}) has started", GetType(), context.Self);
            }
        }
    }
}
