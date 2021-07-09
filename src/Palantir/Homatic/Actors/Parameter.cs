using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace Palantir.Homatic.Actors
{
    public class Parameter : IActor
    {
        private readonly string identifier;
        private readonly DeviceInformation parentDevice;
        private readonly ChannelInformation parentChannel;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<Parameter> logger;

        private ParameterInformation parameterInformation;
        private ParameterValueChanged current;

        public Parameter(string identifier,
            DeviceInformation parentDevice,
            ChannelInformation parentChannel,
            IHttpClientFactory httpClientFactory, ILogger<Parameter> logger)
        {
            this.identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            this.parentDevice = parentDevice ?? throw new ArgumentNullException(nameof(parentDevice));
            this.parentChannel = parentChannel ?? throw new ArgumentNullException(nameof(parentChannel));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Started:
                    await this.OnStarted()
                        .ConfigureAwait(false);
                    break;
                case ParameterValueChanged msg:
                    this.OnValueChanged(context, msg);
                    break;
            }
        }

        private async Task OnStarted()
        {
            try
            {
                var httpClient = this.httpClientFactory.CreateClient();

                this.parameterInformation = await httpClient.GetFromJsonAsync<ParameterInformation>($"http://192.168.2.101:2121/device/{this.identifier}").ConfigureAwait(false);

                this.logger.LogInformation("parameter {identifier} started", this.identifier);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "unable to start parameter {identifier}", this.identifier);
            }
        }

        private void OnValueChanged(IContext context, ParameterValueChanged msg)
        {
            try
            {
                this.current = msg;

                using var hasher = SHA1.Create();

                var fingerPrint = $"{this.identifier}/{msg.Timestamp.Ticks}";

                var value = (JsonElement)msg.Value;

                object convertedValue = this.parameterInformation.Type switch
                {
                    "INTEGER" => value.GetInt32(),
                    "FLOAT" => value.GetDouble(),
                    "BOOL" => value.GetBoolean(),
                    "ENUM" => value.GetInt32(),
                    _ => throw new Exception($"unexpected type {this.parameterInformation.Type}"),
                };

                var enrichedMessage = new EnrichedParameterValueChanged(fingerPrint,
                   msg.Timestamp,
                   new(this.parentDevice.Identifier, parentDevice.Title),
                   new(this.parentChannel.Identifier, parentChannel.Title),
                   new(this.parameterInformation.Identifier, this.parameterInformation.Title),
                   convertedValue,
                   msg.Status
                   );

                context.System.EventStream.Publish(enrichedMessage);

                this.logger.LogInformation("received {data}", msg);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "unexpected error while publishing parameter data");
            }
        }
    }
}
