using Microsoft.Extensions.Logging;
using Proto;
using Proto.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Palantir
{
    public class DeviceController : IActor
    {
        private readonly Dictionary<string, PID> devices = new();
        private readonly ILogger<DeviceController> logger;

        public DeviceController(ILogger<DeviceController> logger)
            => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started:
                    await this.OnStarted(context);
                    break;
                case DeviceData msg:
                    this.OnDeviceData(context, msg);
                    break;
                case GetDeviceStates:
                    var tasks = new List<Task<DeviceState>>();

                    foreach (var device in devices)
                    {
                        var task = context.RequestAsync<DeviceState>(device.Value, new GetDeviceState());
                        tasks.Add(task);
                    }

                    await Task.WhenAll(tasks);

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

            var response = await httpClient.GetFromJsonAsync<DeviceQueryResult>("http://192.168.2.101:2121/device");

            foreach (var link in response.Links)
            {
                if (link.IsParentRef)
                    continue;

                var props = context.System.DI().PropsFor<Device>();
                var pid = context.Spawn(props);

                context.Send(pid, new InitializeDevice(link.Href));

                this.devices.Add(link.Href, pid);
            }
        }

        private void OnDeviceData(IContext context, DeviceData msg)
        {
            var device = this.devices[msg.Device];
            context.Forward(device);
            this.logger.LogInformation($"forwarded device data to device {device.Id}");
        }
    }
}
