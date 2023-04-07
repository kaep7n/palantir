using Microsoft.Extensions.Logging;
using Palantir.Homatic.Http;
using Proto;
using Proto.DependencyInjection;

namespace Palantir.Homatic.Actors;

public class HomaticActor : IActor
{
    private readonly PID root;
    private readonly HomaticHttpClient homaticClient;
    private readonly ILogger<HomaticActor> logger;

    private readonly Dictionary<string, PID> devices = new();

    private PID? mqtt;

    public HomaticActor(PID root, HomaticHttpClient homaticClient, ILogger<HomaticActor> logger)
    {
        this.root = root ?? throw new ArgumentNullException(nameof(root));
        this.homaticClient = homaticClient ?? throw new ArgumentNullException(nameof(homaticClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ReceiveAsync(IContext context)
    {
        if (context.Message is Started)
        {
            try
            {
                this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);

                var devices = await this.homaticClient.GetDevicesAsync();

                foreach (var link in devices.Links)
                {
                    if (link.Href == "..")
                        continue;

                    var props = context.System.DI().PropsFor<HomaticDeviceActor>(link.Href);
                    var pid = context.Spawn(props);

                    this.devices.Add(link.Href, pid);
                }

                var mqttProps = context.System.DI().PropsFor<HomaticMqttActor>();
                this.mqtt = context.Spawn(mqttProps);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "HomaticActor");
            }
        }
        if (context.Message is Join)
        {
            context.Forward(this.root);
        }
        if (context.Message is ParameterValueChanged pvc)
        {
            var device = this.devices[pvc.Device];
            context.Forward(device);
        }
        if (context.Message is Stopped)
        {
            this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);
        }
    }
}
