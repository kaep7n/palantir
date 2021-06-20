using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Palantir.Homatic
{
    public class DeviceController : IActor
    {
        private readonly Dictionary<string, PID> devices = new();
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<DeviceController> logger;

        public DeviceController(IServiceScopeFactory serviceScopeFactory, ILogger<DeviceController> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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

                using var scope = this.serviceScopeFactory.CreateScope();

                var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Device>>();

                var props = Props.FromProducer(() => new Device(link.Href, httpClientFactory, logger));
                var pid = context.Spawn(props);
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
