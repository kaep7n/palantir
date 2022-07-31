using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using Proto;
using System.Text.Json;

namespace Palantir
{
    public class HomaticMqttActor : IActor
    {
        private readonly ILogger<HomaticMqttActor> logger;
        private ActorSystem system;
        private PID? parent;

        public HomaticMqttActor(ILogger<HomaticMqttActor> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
                parent = context.Parent;
                system = context.System;
                logger.LogInformation("{type} ({pid}) has started", GetType(), context.Self);

                await StartMqtt();
            }
            if (context.Message is Stopped)
            {
                logger.LogInformation("{type} ({pid}) has started", GetType(), context.Self);
            }
        }

        private async Task StartMqtt()
        {
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("palantir")
                    .WithTcpServer("192.168.2.101")
                    .Build())
                .Build();

            var mqttClient = new MqttFactory().CreateManagedMqttClient();
            await mqttClient.SubscribeAsync(new[]
            {
                new MqttTopicFilterBuilder()
                .WithTopic("device/status/#")
                .Build()
            }).ConfigureAwait(false);

            mqttClient.DisconnectedAsync += this.MqttClient_DisconnectedAsync;
            mqttClient.ConnectedAsync += this.MqttClient_ConnectedAsync;
            mqttClient.ApplicationMessageReceivedAsync += this.MqttClient_ApplicationMessageReceivedAsync;

            await mqttClient.StartAsync(options).ConfigureAwait(false);
        }

        private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            var topicPaths = arg.ApplicationMessage.Topic.Split("/");

            var device = topicPaths[2];
            var channel = topicPaths[3];
            var type = topicPaths[4];

            var dataString = arg.ApplicationMessage.ConvertPayloadToString();
            var data = JsonSerializer.Deserialize<VeapMessage>(dataString);

            var deviceData = new ParameterValueChanged(device, channel, type,
                DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp),
                data.Value,
                data.Status
            );

            system.Root.Send(parent, deviceData);

            return Task.CompletedTask;
        }

        private Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            this.logger.LogInformation("connected");

            return Task.CompletedTask;
        }

        private Task MqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            this.logger.LogWarning("disconnected");

            return Task.CompletedTask;
        }
    }

}
