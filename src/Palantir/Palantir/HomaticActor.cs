using Proto;
using Proto.DependencyInjection;

namespace Palantir
{
    public class HomaticActor : IActor
    {
        private readonly HomaticHttpClient homaticClient;
        private readonly ILogger<HomaticActor> logger;

        private Dictionary<string, PID> devices = new Dictionary<string, PID>();
        private PID mqtt;

        public HomaticActor(
            HomaticHttpClient homaticClient,
            ILogger<HomaticActor> logger)
        {
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

                    var devices = await homaticClient.GetDevicesAsync();

                    foreach (var link in devices.Links)
                    {
                        if (link.Href == "..")
                            continue;

                        var props = context.System.DI().PropsFor<HomaticDeviceActor>(link.Href);
                        var pid = context.Spawn(props);

                        this.devices.Add(link.Href, pid);
                    }

                    var mqttProps = context.System.DI().PropsFor<HomaticMqttActor>();
                    mqtt = context.Spawn(mqttProps);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "HomaticActor");
                }
            }
            if (context.Message is ParameterValueChanged pvc)
            {
                logger.LogInformation("forwarding to device {device}", pvc.Device);

                var device = this.devices[pvc.Device];
                context.Forward(device);
            }
            if (context.Message is Stopped)
            {
                logger.LogInformation("{type} ({pid}) has started", GetType(), context.Self);
            }
        }
    }
}
