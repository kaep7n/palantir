using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Palantir.Homatic.Actors
{
    public class Parameter : IActor
    {
        private readonly string identifier;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<Parameter> logger;

        private ParameterInformation parameterInformation;
        private DeviceParameterValue current;

        public Parameter(string identifier, IHttpClientFactory httpClientFactory, ILogger<Parameter> logger)
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
                    await this.OnStarted().ConfigureAwait(false);
                    break;
                case DeviceParameterValue msg:
                    this.OnDeviceData(context, msg);
                    break;
                default:
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

        private void OnDeviceData(IContext context, DeviceParameterValue msg)
        {
            try
            {
                this.current = msg;

                var value = (JsonElement)msg.Value;

                object convertedValue = this.parameterInformation.Type switch
                {
                    "INTEGER" => value.GetInt32(),
                    "FLOAT" => value.GetDouble(),
                    "BOOL" => value.GetBoolean(),
                    "ENUM" => value.GetString(),
                    _ => throw new Exception($"unexpected type {this.parameterInformation.Type}"),
                };

                context.System.EventStream.Publish(msg with { Value = convertedValue });

                this.logger.LogInformation("received {data}", msg);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "unexpected error while publishing parameter data");
            }
        }
    }
}
