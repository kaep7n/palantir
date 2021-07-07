using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Palantir.Homatic.Actors
{
    public class Channel : IActor
    {
        private readonly string identifier;
        private readonly IParameterFactory parameterFactory;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<Channel> logger;

        private readonly Dictionary<string, PID> parameters = new();

        private ChannelInformation channelInformation;

        public Channel(string identifier, IParameterFactory parameterFactory, IHttpClientFactory httpClientFactory, ILogger<Channel> logger)
        {
            this.identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            this.parameterFactory = parameterFactory ?? throw new ArgumentNullException(nameof(parameterFactory));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started:
                    await this.OnStarted(context).ConfigureAwait(false);
                    break;
                case DeviceParameterValue msg:
                    this.OnDeviceData(context, msg);
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

                this.channelInformation = await httpClient.GetFromJsonAsync<ChannelInformation>($"http://192.168.2.101:2121/device/{this.identifier}").ConfigureAwait(false);

                foreach (var parameterLink in this.channelInformation.Links)
                {
                    if (parameterLink.IsParentRef)
                        continue;
                    if (parameterLink.Rel != "parameter")
                        continue;

                    var props = this.parameterFactory.CreateProps($"{this.identifier}/{parameterLink.Href}");
                    var pid = context.Spawn(props);

                    this.parameters.Add(parameterLink.Href, pid);
                }

                this.logger.LogInformation("channel {identifier} started with {parameterCount} parameters", this.identifier, this.parameters.Count);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "unable to start channel {identifier}", this.identifier);
            }
        }

        private void OnDeviceData(IContext context, DeviceParameterValue msg)
        {
            if (!this.parameters.TryGetValue(msg.Parameter, out var parameterPid))
                this.logger.LogWarning("parameter {identifier} does not exist", msg.Device);

            context.Forward(parameterPid);
            this.logger.LogTrace($"forwarded device data to parameter {parameterPid}");
        }
    }
}
