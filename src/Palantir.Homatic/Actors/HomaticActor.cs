using Microsoft.Extensions.Logging;
using Proto;
using Proto.DependencyInjection;

namespace Palantir.Homatic.Actors;

public class HomaticActor : IActor
{
    private readonly ILogger<HomaticActor> logger;

    private readonly Dictionary<string, PID> devices = new();

    private PID? mqtt;
    private PID http;

    public HomaticActor(ILogger<HomaticActor> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ReceiveAsync(IContext context)
    {
        if (context.Message is Started)
        {
            this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);
            //try
            //{


            //    var devices = await this.homaticClient.GetDevicesAsync();

            //    foreach (var link in devices.Links)
            //    {
            //        if (link.Href == "..")
            //            continue;

            //        var props = context.System.DI().PropsFor<HomaticDeviceActor>(link.Href);
            //        var pid = context.Spawn(props);

            //        this.devices.Add(link.Href, pid);
            //    }

            //    var mqttProps = context.System.DI().PropsFor<HomaticMqttActor>();
            //    this.mqtt = context.Spawn(mqttProps);
            //}
            //catch (Exception exception)
            //{
            //    this.logger.LogError(exception, "HomaticActor");
            //}

            var httpProps = context.System.DI().PropsFor<HttpReceiverArctor>();
            this.http = context.Spawn(httpProps);

            context.Request(this.http, new HttpRequest(new Uri("http://192.168.2.101:2121/device"), typeof(Devices)), context.Self);
        }
        if (context.Message is HttpResponse response)
        {

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
