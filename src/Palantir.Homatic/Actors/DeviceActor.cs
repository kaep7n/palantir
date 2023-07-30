using Microsoft.Extensions.Logging;
using Proto;
using Proto.DependencyInjection;

namespace Palantir.Homatic.Actors;

public class DeviceActor(PID apiPool, string id, ILogger<DeviceActor> logger) : BaseActor(apiPool, id, logger)
{
    private readonly Dictionary<string, PID> channels = new();

    public override Task ReceiveAsync(IContext context)
    {
        base.ReceiveAsync(context);

        return context.Message switch
        {
            GetDeviceResult msg => this.OnGetDeviceResult(context, msg),
            ParameterValueChanged msg => this.OnParameterValueChanged(context, msg),
            _ => Task.CompletedTask
        };
    }

    protected override Task OnStarted(IContext context)
    {
        context.Request(this.apiPool, new GetDevice(this.id), context.Self);

        return base.OnStarted(context);
    }

    private Task OnParameterValueChanged(IContext context, ParameterValueChanged pvc)
    {
        if (this.channels.TryGetValue(pvc.Channel, out var channel))
        {
            context.Forward(channel);
        }
        else
        {
            this.logger.LogWarning("could not find channel '{channel}' on device '{device}'", pvc.Channel, this.id);
        }

        return Task.CompletedTask;
    }

    private Task OnGetDeviceResult(IContext context, GetDeviceResult result)
    {
        foreach (var link in result.Device.Links)
        {
            if (link.Href == "..")
                continue;

            var props = context.System.DI().PropsFor<ChannelActor>(this.apiPool, $"{this.id}/{link.Href}");
            var pid = context.Spawn(props);

            this.channels.Add(link.Href, pid);
        }

        return Task.CompletedTask;
    }
}
