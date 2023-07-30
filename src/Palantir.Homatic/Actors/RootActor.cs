using Microsoft.Extensions.Logging;
using Proto;
using Proto.DependencyInjection;
using Proto.Router;

namespace Palantir.Homatic.Actors;

public class RootActor : IActor
{
    private readonly ILogger<RootActor> logger;

    private readonly Dictionary<string, PID> devices = new();

    private PID apiPool;

    private PID mqtt;

    public RootActor(ILogger<RootActor> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task ReceiveAsync(IContext context)
    {
        if (context.Message is Started)
        {
            this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);

            var mqttProps = context.System.DI().PropsFor<MqttActor>();
            this.mqtt = context.Spawn(mqttProps);

            context.Send(this.mqtt, new Connect());

            var apiProps = context.System.DI().PropsFor<ApiActor>();
            var apiPoolProps = context.NewRoundRobinPool(apiProps, 5);
            this.apiPool = context.Spawn(apiPoolProps);

            context.Request(this.apiPool, new GetDevices(), context.Self);
        }
        else if (context.Message is GetDevicesResult result)
        {
            foreach (var link in result.Devices.Links)
            {
                if (link.Href == "..")
                    continue;

                var props = context.System.DI().PropsFor<DeviceActor>(this.apiPool, link.Href);
                var pid = context.Spawn(props);

                this.devices.Add(link.Href, pid);
            }
        }
        else if (context.Message is ParameterValueChanged pvc)
        {
            var device = this.devices[pvc.Device];
            context.Forward(device);
        }
        else if (context.Message is Stopped)
        {
            context.Send(this.mqtt, new Disconnect());
            this.mqtt?.Stop(context.System);

            this.apiPool?.Stop(context.System);

            this.logger.LogDebug("{type} ({pid}) has stopped", this.GetType(), context.Self);
        }

        return Task.CompletedTask;
    }
}
