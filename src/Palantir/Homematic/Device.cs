using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Palantir.Homatic
{
    public class Device : IActor
    {
        private readonly string identifier;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<Device> logger;
        private DeviceInformation information;

        public Device(string identifier, IHttpClientFactory httpClientFactory, ILogger<Device> logger)
        {
            this.identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started:
                    await this.OnStarted();
                    break;
                case DeviceData:
                    this.OnDeviceData(context);
                    break;
                default:
                    break;
            }
        }

        private async Task OnStarted()
        {
            var httpClient = this.httpClientFactory.CreateClient();

            this.information = await httpClient.GetFromJsonAsync<DeviceInformation>($"http://192.168.2.101:2121/device/{this.identifier}");
        }

        private void OnDeviceData(IContext context)
        {
            this.logger.LogInformation($"received: {context.Message}");
            context.System.EventStream.Publish(context.Message);
        }
    }
}
