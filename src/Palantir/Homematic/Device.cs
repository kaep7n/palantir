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
                case DeviceData msg:
                    this.OnDeviceData(context, msg);
                    break;
                case GetDeviceState:
                    context.Respond(new DeviceState(this.information, null));
                    break;
                default:
                    break;
            }
        }

        private async Task OnStarted(IContext context)
        {
            try
            {
                var httpClient = this.httpClientFactory.CreateClient();
                this.information = await httpClient.GetFromJsonAsync<DeviceInformation>($"http://192.168.2.101:2121/device/{this.identifier}");

                foreach (var channelLink in this.information.Links)
                {
                    if (channelLink.IsParentRef)
                        continue;

                    var props = this.channelFactory.CreateProps($"{this.identifier}/{channelLink.Href}");
                    var pid = context.Spawn(props);

                    this.channels.Add(channelLink.Href, pid);
                }

                this.logger.LogInformation("device {identifier} started with {channelCount} channels", this.identifier, this.channels.Count);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "unable to start device {identifier}", this.identifier);
            }
        }

        private void OnDeviceData(IContext context, DeviceData msg)
        {
            if (!this.channels.TryGetValue(msg.Channel, out var channelPid))
                this.logger.LogWarning("channel {identifier} does not exist", msg.Device);

            context.Forward(channelPid);
            this.logger.LogTrace($"forwarded device data to channel {channelPid}");
        }
    }
}
