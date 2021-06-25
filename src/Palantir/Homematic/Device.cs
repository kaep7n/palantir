using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Palantir
{
    public class Device : IActor
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<Device> logger;

        private string identifier;
        private DeviceInformation information;

        private readonly Dictionary<int, Channel> channels = new();

        public Device(IHttpClientFactory httpClientFactory, ILogger<Device> logger)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case InitializeDevice msg:
                    await this.OnInitializedAsync(msg);
                    break;
                case DeviceData:
                    this.OnDeviceData(context);
                    break;
                case GetDeviceState:
                    context.Respond(new DeviceState(this.information, this.channels.Values));
                    break;
                default:
                    break;
            }
        }

        private async Task OnInitializedAsync(InitializeDevice msg)
        {
            this.identifier = msg.Identifier;

            var httpClient = this.httpClientFactory.CreateClient();
            this.information = await httpClient.GetFromJsonAsync<DeviceInformation>($"http://192.168.2.101:2121/device/{this.identifier}");
        }

        private void OnDeviceData(IContext context)
        {
            this.logger.LogInformation($"received: {context.Message}");

            if (context.Message is DeviceData data)
            {
                if (!this.channels.TryGetValue(data.Channel, out var channel))
                {
                    channel = new Channel(data.Channel, new Dictionary<string, Data>());
                    this.channels.Add(data.Channel, channel);
                }

                channel.Params[data.Type] = data.Data;
            }

            context.System.EventStream.Publish(context.Message);
        }
    }
}
