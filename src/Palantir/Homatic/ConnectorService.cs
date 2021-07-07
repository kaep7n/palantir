using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using Palantir.Homatic.Actors;
using Proto;
using Proto.DependencyInjection;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Palantir.Homatic
{
    public class ConnectorService : IHostedService
    {
        private readonly ActorSystem actorSystem;
        private readonly ILogger<ConnectorService> logger;
        private PID homaticRoot;

        public ConnectorService(ActorSystem actorSystem, ILogger<ConnectorService> logger)
        {
            this.actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.homaticRoot = this.actorSystem.Root.Spawn(
                this.actorSystem.DI().PropsFor<Root>()
            );

            await this.StartMqtt(this.actorSystem, this.homaticRoot).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<TResult> RequestAsync<TResult>(object message)
            => this.actorSystem.Root.RequestAsync<TResult>(this.homaticRoot, message);

        private async Task StartMqtt(ActorSystem system, PID root)
        {
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("palantir")
                    .WithTcpServer("192.168.2.101")
                    .Build())
                .Build();

            var mqttClient = new MqttFactory().CreateManagedMqttClient();
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic("device/status/#")
                .Build()).ConfigureAwait(false);

            mqttClient.UseDisconnectedHandler(_ =>
            {
                this.logger.LogWarning("disconnected");
            });

            mqttClient.UseConnectedHandler(_ =>
            {
                this.logger.LogInformation("connected");
            });

            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                var topicPaths = e.ApplicationMessage.Topic.Split("/");

                var device = topicPaths[2];
                var channel = topicPaths[3];
                var type = topicPaths[4];

                var dataString = e.ApplicationMessage.ConvertPayloadToString();
                var data = JsonSerializer.Deserialize<VeapMessage>(dataString);

                var deviceData = new DeviceParameterValue(device, channel, type,
                    DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp),
                    data.Value,
                    data.Status
                );

                system.Root.Send(root, deviceData);
            });

            await mqttClient.StartAsync(options).ConfigureAwait(false);
        }
    }
}
