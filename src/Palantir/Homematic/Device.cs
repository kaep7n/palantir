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

        private readonly string identifier;
        private readonly IChannelFactory channelFactory;
        private DeviceInformation information;

        private readonly Dictionary<string, PID> channels = new();

        public Device(string identifier, IChannelFactory channelFactory, IHttpClientFactory httpClientFactory, ILogger<Device> logger)
        {
            this.identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            this.channelFactory = channelFactory ?? throw new ArgumentNullException(nameof(channelFactory));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started:
                    await this.OnStarted(context);
                    break;
                case DeviceData:
                    this.OnDeviceData(context);
                    break;
                case GetDeviceState:
                    throw new NotSupportedException("currently not supported... see code");
                    //context.Respond(new DeviceState(this.information, this.channels.Values));
                    break;
                default:
                    break;
            }
        }

        private async Task OnStarted(IContext context)
        {
            var httpClient = this.httpClientFactory.CreateClient();
            this.information = await httpClient.GetFromJsonAsync<DeviceInformation>($"http://192.168.2.101:2121/device/{this.identifier}");
        
            foreach(var channelLink in this.information.Links)
            {
                if (channelLink.IsParentRef)
                    continue;

                var props = this.channelFactory.CreateProps($"{this.identifier}/{channelLink.Href}");
                var pid = context.Spawn(props);

                this.channels.Add(channelLink.Href, pid);
            }
        }

        private void OnDeviceData(IContext context)
        {
            this.logger.LogInformation($"received: {context.Message}");

            //if (context.Message is DeviceData data)
            //{
            //    if (!this.channels.TryGetValue(data.Channel, out var channel))
            //    {
            //        channel = new ChannelData(data.Channel, new Dictionary<string, Data>());
            //        this.channels.Add(data.Channel, channel);
            //    }

            //    channel.Params[data.Type] = data.Data;
            //}

            context.System.EventStream.Publish(context.Message);
        }
    }
}
