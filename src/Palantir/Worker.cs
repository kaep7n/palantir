using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using Proto;
using Proto.DependencyInjection;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Palantir
{
    public class Worker : IHostedService
    {
        private readonly ActorSystem actorSystem;
        private readonly ILogger<Worker> logger;
        private PID persistorGroup;
        public PID homaticRoot;

        public Worker(ActorSystem actorSystem, ILogger<Worker> logger)
        {
            this.actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.persistorGroup = this.actorSystem.Root.Spawn(
                this.actorSystem.DI().PropsFor<PersistorGroup>()
            );
            this.homaticRoot = this.actorSystem.Root.Spawn(
                this.actorSystem.DI().PropsFor<HomaticRoot>()
            );

            await this.StartMqtt(this.actorSystem, this.homaticRoot);
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        private async Task StartMqtt(ActorSystem system, PID root)
        {

            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("funky")
                    .WithTcpServer("192.168.2.101")
                    .Build())
                .Build();

            var mqttClient = new MqttFactory().CreateManagedMqttClient();
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic("device/status/#")
                .Build());

            mqttClient.UseDisconnectedHandler(e =>
            {
                this.logger.LogWarning("disconnected");
            });

            mqttClient.UseConnectedHandler(e =>
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
                var data = JsonSerializer.Deserialize<Data>(dataString);

                system.Root.Send(root, new DeviceData(device, int.Parse(channel), type, data));
            });

            await mqttClient.StartAsync(options);
        }
    }
}
