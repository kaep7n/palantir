using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using Proto;
using System.Text.Json;

namespace Palantir.Homatic.Actors;

public class MqttActor(ILogger<MqttActor> logger) : IActor
{
    private readonly ILogger<MqttActor> logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private ActorSystem system;
    private PID? parent;
    private IManagedMqttClient mqttClient;

    public Task ReceiveAsync(IContext context)
        => context.Message switch
        {
            Started => this.OnStarted(context),
            Connect => this.OnConnect(),
            Disconnect => this.OnDisconnect(),
            Stopped => this.OnStopped(context),
            _ => this.OnUnexpectedMessage(context)
        };

    private Task OnStarted(IContext context)
    {
        this.parent = context.Parent;
        this.system = context.System;

        this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);

        return Task.CompletedTask;
    }

    private async Task OnConnect()
    {
        var options = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithClientId("palantir")
                .WithTcpServer("localhost")
                .Build())
            .Build();

        this.mqttClient = new MqttFactory().CreateManagedMqttClient();
        await this.mqttClient.SubscribeAsync(new[]
        {
            new MqttTopicFilterBuilder()
            .WithTopic("device/status/#")
            .Build()
        }).ConfigureAwait(false);

        this.mqttClient.DisconnectedAsync += this.MqttClient_DisconnectedAsync;
        this.mqttClient.ConnectedAsync += this.MqttClient_ConnectedAsync;
        this.mqttClient.ApplicationMessageReceivedAsync += this.MqttClient_ApplicationMessageReceivedAsync;

        await this.mqttClient.StartAsync(options).ConfigureAwait(false);
    }

    private async Task OnDisconnect()
    {
        await this.mqttClient.StopAsync();
    }

    private async Task OnStopped(IContext context)
    {
        await this.mqttClient.StopAsync();
        this.logger.LogDebug("{type} ({pid}) has stopped", this.GetType(), context.Self);
    }

    private Task OnUnexpectedMessage(IContext context)
    {
        this.logger.LogInformation("received unexpected message {@msg}", context.Message);

        return Task.CompletedTask;
    }

    private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        if (this.parent is null)
            throw new InvalidOperationException("No parent to send data to.");

        var topicPaths = arg.ApplicationMessage.Topic.Split("/");

        var device = topicPaths[2];
        var channel = topicPaths[3];
        var type = topicPaths[4];

        var dataString = arg.ApplicationMessage.ConvertPayloadToString();
        var data = JsonSerializer.Deserialize<VeapMessage>(dataString)
            ?? throw new InvalidOperationException($"could not deserialize {dataString} to {typeof(VeapMessage)}");

        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp);
        var value = data.Value.ValueKind switch
        {
            JsonValueKind.Number => data.Value.GetDouble(),
            JsonValueKind.String => data.Value.GetString(),
            JsonValueKind.False => data.Value.GetBoolean(),
            JsonValueKind.True => (object)data.Value.GetBoolean(),
            _ => throw new ArgumentException($"Unexpected Value Kind {data.Value.ValueKind}")
        } ?? throw new InvalidOperationException($"unable to convert json value {data.Value} from type {data.Value.ValueKind}.");

        var pvc = new ParameterValueChanged(device, channel, type, timestamp, value, data.Status);

        this.system.Root.Send(this.parent, pvc);

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

public record Connect();

public record Disconnect();