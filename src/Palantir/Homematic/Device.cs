using Microsoft.Extensions.Logging;
using Palantir.Homematic;
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

        private readonly Dictionary<int, ChannelData> channels = new();

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
        
            foreach(var channelLink in this.information.Links)
            {
                if (channelLink.IsParentRef) 
                    continue;

                var response = await httpClient.GetAsync($"http://192.168.2.101:2121/device/{this.identifier}/{channelLink.Href}");
                var responseString = await response.Content.ReadAsStringAsync();
                var channelInformation = await httpClient.GetFromJsonAsync<ChannelInformation>($"http://192.168.2.101:2121/device/{this.identifier}/{channelLink.Href}");

                foreach (var parameterLink in channelInformation.Links)
                {
                    if (parameterLink.IsParentRef)
                        continue;

                    var parameterInformation = await httpClient.GetFromJsonAsync<ParameterInformation>($"http://192.168.2.101:2121/device/{this.identifier}/{channelLink.Href}/{parameterLink.Href}");
                }
            }
        }

        private void OnDeviceData(IContext context)
        {
            this.logger.LogInformation($"received: {context.Message}");

            if (context.Message is DeviceData data)
            {
                if (!this.channels.TryGetValue(data.Channel, out var channel))
                {
                    channel = new ChannelData(data.Channel, new Dictionary<string, Data>());
                    this.channels.Add(data.Channel, channel);
                }

                channel.Params[data.Type] = data.Data;
            }

            context.System.EventStream.Publish(context.Message);
        }
    }
}
