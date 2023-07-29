using Microsoft.Extensions.Logging;
using Proto;
using Proto.DependencyInjection;

namespace Palantir.Homatic.Actors;

public class DeviceActor : IActor
{
    private readonly PID apiPool;
    private readonly string id;
    private readonly ILogger<DeviceActor> logger;

    private readonly Dictionary<string, PID> channels = new Dictionary<string, PID>();

    public DeviceActor(
        PID apiPool,
        string id,
        ILogger<DeviceActor> logger)
    {
        this.apiPool = apiPool ?? throw new ArgumentNullException(nameof(apiPool));
        this.id = id ?? throw new ArgumentNullException(nameof(id));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task ReceiveAsync(IContext context)
    {
        if (context.Message is Started)
        {
            this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);

            context.Request(this.apiPool, new GetDevice(this.id), context.Self);
        }
        else if (context.Message is GetDeviceResult result)
        {
            foreach (var link in result.Device.Links)
            {
                if (link.Href == "..")
                    continue;

                var props = context.System.DI().PropsFor<ChannelActor>(this.apiPool, this.id, link.Href);
                var pid = context.Spawn(props);

                this.channels.Add(link.Href, pid);
            }
        }
        else if (context.Message is ParameterValueChanged pvc)
        {
            var channel = this.channels[pvc.Channel];

            context.Forward(channel);
        }
        else if (context.Message is JoinRoom)
        {
            context.Forward(context.Parent);
        }
        else if (context.Message is Stopped)
        {
            this.logger.LogDebug("{type} ({pid}) has stopped", this.GetType(), context.Self);
        }

        return Task.CompletedTask;
    }
}
