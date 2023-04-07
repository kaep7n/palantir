using Microsoft.Extensions.Logging;
using Palantir.Homatic.Http;
using Proto;
using Proto.DependencyInjection;

namespace Palantir.Homatic.Actors;

public class HomaticDeviceActor : IActor
{
    private readonly string id;
    private readonly HomaticHttpClient homaticClient;
    private readonly ILogger<HomaticDeviceActor> logger;

    private readonly Dictionary<string, PID> channels = new Dictionary<string, PID>();

    public HomaticDeviceActor(
        string id,
        HomaticHttpClient homaticClient,
        ILogger<HomaticDeviceActor> logger)
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
                this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);

                var device = await this.homaticClient.GetDeviceAsync(this.id);

                foreach (var link in device.Links)
                {
                    if (link.Href == "..")
                        continue;

                    var props = context.System.DI().PropsFor<HomaticDeviceChannelActor>(this.id, link.Href);
                    var pid = context.Spawn(props);

                    this.channels.Add(link.Href, pid);
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "HomaticDeviceActor");
            }
        }
        if (context.Message is ParameterValueChanged pvc)
        {
            var channel = this.channels[pvc.Channel];

            context.Forward(channel);
        }
        if (context.Message is Join)
        {
            context.Forward(context.Parent);
        }
        if (context.Message is Stopped)
        {
            this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);
        }
    }
}
