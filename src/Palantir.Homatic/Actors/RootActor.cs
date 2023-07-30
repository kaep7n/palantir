using Microsoft.Extensions.Logging;
using Proto;
using Proto.DependencyInjection;
using Proto.Router;

namespace Palantir.Homatic.Actors;

public class RootActor(ILogger<RootActor> logger) : IActor
{
    private PID? apiPool;
    private PID? mqtt;

    private readonly Dictionary<string, PID> devices = new();

    private readonly ILogger<RootActor> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task ReceiveAsync(IContext context)
        => context.Message switch
        {
            Started => this.OnStarted(context),
            GetDevicesResult result => this.OnGetDevicesResult(context, result),
            ParameterValueChanged pvc => this.OnParameterValueChanged(context, pvc),
            Stopped => this.OnStopped(context),
            _ => Task.CompletedTask
        };

    private Task OnStarted(IContext context)
    {
        this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);

        var mqttProps = context.System.DI().PropsFor<MqttActor>();
        this.mqtt = context.Spawn(mqttProps);

        context.Send(this.mqtt, new Connect());

        var apiProps = context.System.DI().PropsFor<ApiActor>();
        var apiPoolProps = context.NewRoundRobinPool(apiProps, 5);
        this.apiPool = context.Spawn(apiPoolProps);

        context.Request(this.apiPool, new GetDevices(), context.Self);

        return Task.CompletedTask;
    }

    private Task OnGetDevicesResult(IContext context, GetDevicesResult result)
    {
        foreach (var link in result.Devices.Links)
        {
            if (link.Href == "..")
                continue;

            var props = context.System.DI().PropsFor<DeviceActor>(this.apiPool!, link.Href);
            var pid = context.Spawn(props);

            this.devices.Add(link.Href, pid);
        }

        return Task.CompletedTask;
    }

    private Task OnParameterValueChanged(IContext context, ParameterValueChanged pvc)
    {
        if (this.devices.TryGetValue(pvc.Device, out var device))
        {
            context.Forward(device);
        }
        else
        {
            this.logger.LogWarning("could not find device '{device}'", pvc.Device);
        }

        return Task.CompletedTask;
    }

    private Task OnStopped(IContext context)
    {
        context.Send(this.mqtt!, new Disconnect());
        this.mqtt?.Stop(context.System);

        this.apiPool?.Stop(context.System);

        this.logger.LogDebug("{type} ({pid}) has stopped", this.GetType(), context.Self);

        return Task.CompletedTask;
    }
}
