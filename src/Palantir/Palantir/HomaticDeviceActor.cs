using Proto;
using Proto.DependencyInjection;

namespace Palantir
{
    public class HomaticDeviceActor : IActor
    {
        private readonly string id;
        private readonly HomaticHttpClient homaticClient;
        private readonly ILogger<HomaticActor> logger;

        private readonly Dictionary<string, PID> channels = new Dictionary<string, PID>();

        public HomaticDeviceActor(
            string id,
            HomaticHttpClient homaticClient,
            ILogger<HomaticActor> logger)
        {
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
                    logger.LogInformation("{type} ({pid}) has started", GetType(), context.Self);

                    var device = await homaticClient.GetDeviceAsync(id);

                    foreach (var link in device.Links)
                    {
                        if (link.Href == "..")
                            continue;

                        var props = context.System.DI().PropsFor<HomaticDeviceChannelActor>(id, link.Href);
                        var pid = context.Spawn(props);

                        channels.Add(link.Href, pid);
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "HomaticDeviceActor");
                }
            }
            if (context.Message is Stopped)
            {
                logger.LogInformation("{type} ({pid}) has started", GetType(), context.Self);
            }
        }
    }
}
