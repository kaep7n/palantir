﻿using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Palantir.Homematic
{
    public class Parameter : IActor
    {
        private readonly string identifier;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<Parameter> logger;

        private ParameterInformation parameterInformation;
        private DeviceData current;

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
                    await this.OnStarted();
                    break;
                case DeviceData msg:
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

                this.parameterInformation = await httpClient.GetFromJsonAsync<ParameterInformation>($"http://192.168.2.101:2121/device/{this.identifier}");

                this.logger.LogInformation("parameter {identifier} started", this.identifier);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "unable to start parameter {identifier}", this.identifier);
            }
        }

        private void OnDeviceData(IContext context, DeviceData msg)
        {
            this.current = msg;

            var value = (JsonElement)msg.Value;

            object valueResult = null;

            switch (value.ValueKind)
            {
                case JsonValueKind.Undefined:
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                case JsonValueKind.Null:
                    break;
                case JsonValueKind.String:
                    valueResult = value.GetString();
                    break;
                case JsonValueKind.Number:
                    valueResult = value.GetDouble();
                    break;
                case JsonValueKind.True:
                    valueResult = value.GetBoolean();
                    break;
                case JsonValueKind.False:
                    valueResult = value.GetBoolean();
                    break;
                default:
                    throw new Exception("unexpected value kind");
            }

            context.System.EventStream.Publish(msg with { Value = valueResult });

            this.logger.LogInformation("received {deviceData}", msg);
        }
    }
}
