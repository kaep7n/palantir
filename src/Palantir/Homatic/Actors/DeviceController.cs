using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Palantir.Homatic.Actors
{
    public class DeviceController : IActor
    {
        private readonly Dictionary<string, PID> devices = new();
        private readonly IDeviceFactory deviceFactory;
        private readonly ILogger<DeviceController> logger;

        public DeviceController(IDeviceFactory deviceFactory, ILogger<DeviceController> logger)
        {
            this.deviceFactory = deviceFactory ?? throw new ArgumentNullException(nameof(deviceFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started:
                    await this.OnStarted(context).ConfigureAwait(false);
                    break;
                case ParameterValueChanged msg:
                    this.OnDeviceData(context, msg);
                    break;
                case GetDeviceStates:
                    var tasks = new List<Task<DeviceState>>();

                    foreach (var device in devices)
                    {
                        var task = context.RequestAsync<DeviceState>(device.Value, new GetDeviceState());
                        tasks.Add(task);
                    }

                    await Task.WhenAll(tasks)
                        .ConfigureAwait(false);

                    context.Respond(new DeviceStates(tasks.Select(t => t.Result)));

                    break;
                default:
                    break;
            }
        }

        private async Task OnStarted(IContext context)
        {
            this.logger.LogInformation("started device controller");
            var httpClient = new HttpClient();

            var response = await httpClient.GetFromJsonAsync<DeviceQueryResult>("http://192.168.2.101:2121/device").ConfigureAwait(false);

            foreach (var link in response.Links)
            {
                if (link.IsParentRef)
                    continue;

                var props = this.deviceFactory.CreateProps(link.Href);
                var pid = context.Spawn(props);

                this.devices.Add(link.Href, pid);
            }
        }

        private void OnDeviceData(IContext context, ParameterValueChanged msg)
        {
            if (!this.devices.TryGetValue(msg.Device, out var devicePid))
                this.logger.LogWarning("device {identifier} does not exist", msg.Device);

            context.Forward(devicePid);
            this.logger.LogTrace($"forwarded device data to device {devicePid}");
        }
    }
}
