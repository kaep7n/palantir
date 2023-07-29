using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using Proto;
using System.Text.Json;

namespace Palantir.Homatic.Actors;

public class MqttActor : IActor
{
    private readonly ILogger<MqttActor> logger;
    private ActorSystem system;
    private PID? parent;

    public MqttActor(ILogger<MqttActor> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ReceiveAsync(IContext context)
    {
        if (context.Message is Started)
        {
            this.parent = context.Parent;
            this.system = context.System;
            this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);

            await this.StartMqtt();
        }
        if (context.Message is Stopped)
        {
            this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);
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
            data.Value.ValueKind switch
            {
                JsonValueKind.Number => data.Value.GetDecimal(),
                JsonValueKind.String => data.Value.GetString(),
                JsonValueKind.False => data.Value.GetBoolean(),
                JsonValueKind.True => data.Value.GetBoolean(),
                _ => throw new ArgumentException($"Unexpected Value Kind {data.Value.ValueKind}")
            },
            data.Status
        );

        this.system.Root.Send(this.parent, deviceData);

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

