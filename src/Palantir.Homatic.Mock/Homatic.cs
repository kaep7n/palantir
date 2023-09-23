using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Server;
using Palantir.Homatic.Mock.Json;
using System.Text.Json;

namespace Palantir.Homatic.Mock;

public sealed class Homatic : IHostedService
{
    private readonly IOptionsMonitor<HomaticOptions> optionsMonitor;
    private readonly ILogger<Homatic> logger;
    private JsonDevices jsonDevices;

    private MqttServer server;

    private System.Threading.Channels.Channel<MqttApplicationMessage> outbox = System.Threading.Channels.Channel.CreateUnbounded<MqttApplicationMessage>();

    public string RootPath => this.optionsMonitor.CurrentValue.RootPath;

    public List<Device> Devices { get; set; } = new List<Device>();

    public Homatic(IOptionsMonitor<HomaticOptions> optionsMonitor, ILogger<Homatic> logger)
    {
        ArgumentNullException.ThrowIfNull(optionsMonitor);
        ArgumentNullException.ThrowIfNull(logger);

        this.optionsMonitor = optionsMonitor;
        this.logger = logger;

        var mqttFactory = new MqttFactory();
        var mqttServerOptions = mqttFactory
            .CreateServerOptionsBuilder()
            .WithDefaultEndpoint()
            .Build();

        this.server = mqttFactory.CreateMqttServer(mqttServerOptions);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("starting homatic");
        var jsonDevicesPath = Path.Combine(this.RootPath, "devices.json");

        this.jsonDevices = JsonSerializer.Deserialize<JsonDevices>(File.ReadAllText(jsonDevicesPath))
            ?? throw new InvalidOperationException($"could not read devices from path '{jsonDevicesPath}'");

        this.Devices = this.ReadDevices(this.jsonDevices);

        await this.server.StartAsync();

        this.logger.LogInformation("started homatic");

        _ = this.ProcessOutbox(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("stopping homatic");

        await this.server.StopAsync();

        this.logger.LogInformation("stopped homatic");
    }


    public JsonDevices GetRaw()
    {
        return this.jsonDevices;
    }

    public Device GetDevice(string deviceId)
        => this.Devices.First(d => d.Identifier == deviceId);

    public Channel GetChannel(string deviceId, string channelId)
        => this.Devices.First(d => d.Identifier == deviceId)
            .Channels.First(c => c.Identifier == channelId);

    public Parameter GetParameter(string deviceId, string channelId, string parameterId)
        => this.Devices.First(d => d.Identifier == deviceId)
            .Channels.First(c => c.Identifier == channelId)
            .Parameters.First(p => p.Identifier == parameterId);

    public Veap GetParameterValue(string deviceId, string channelId, string parameterId)
    {
        var parameter = this.GetParameter(deviceId, channelId, parameterId);

        return new Veap(parameter.CurrentValueChanged.ToUnixTimeMilliseconds(), parameter.CurrentValue, 0);
    }

    public void SetParameterValue(string deviceId, string channelId, string parameterId, object value)
    {
        var parameter = this.GetParameter(deviceId, channelId, parameterId);

        // TODO: set value here
        var payload = new Veap(DateTimeOffset.Now.ToUnixTimeMilliseconds(), value, 0);

        var serializedPayload = JsonSerializer.Serialize(payload);

        var message = new MqttApplicationMessageBuilder()
                    .WithTopic(parameter.MqttStatusTopic)
                    .WithPayload(serializedPayload)
                    .Build();

        if (outbox.Writer.TryWrite(message))
            this.logger.LogInformation("wrote {@message} to outbox", message);
        else
            this.logger.LogWarning("could not write {@message} to outbox", message);


    }

    public void Dispose()
    {
        this.server.Dispose();
    }

    private async Task ProcessOutbox(CancellationToken cancellationToken)
    {
        await foreach (var message in this.outbox.Reader.ReadAllAsync(cancellationToken))
        {
            this.logger.LogInformation("publishing message to topic {topic}", message.Topic);

            await this.server.InjectApplicationMessage(new InjectedMqttApplicationMessage(message)
            {
                SenderClientId = "Mock"
            }, cancellationToken);
        }
    }

    private List<Device> ReadDevices(JsonDevices jsonDevices)
    {
        return jsonDevices.Links.Where(l => l.Rel == "devices")
            .Select(l =>
            {
                var jsonDevicePath = Path.Combine(this.RootPath, "devices", l.Href, "device.json");
                var jsonDevice = JsonSerializer.Deserialize<JsonDevice>(File.ReadAllText(jsonDevicePath))
                    ?? throw new InvalidOperationException($"could not read device from path '{jsonDevicePath}'");

                var channels = this.ReadChannels(l, jsonDevice);

                return new Device(jsonDevice, channels);
            })
            .ToList();
    }

    private List<Channel> ReadChannels(JsonLink jsonDevicesLink, JsonDevice jsonDevice)
    {
        return jsonDevice.Links.Where(l => l.Rel == "channel")
            .Select(l =>
            {
                var jsonChannelPath = Path.Combine(
                    this.RootPath, "devices",
                    jsonDevicesLink.Href, "channels",
                    l.Href, "channel.json"
                );

                var jsonChannel = JsonSerializer.Deserialize<JsonChannel>(File.ReadAllText(jsonChannelPath))
                    ?? throw new InvalidOperationException($"could not read channel from path '{jsonChannelPath}'"); ;

                var parameters = this.ReadParameters(jsonDevicesLink, jsonDevicesLink, jsonChannel);
                var rooms = ReadRooms(jsonChannel);

                return new Channel(jsonChannel, rooms, parameters);
            }).ToList();
    }

    private List<Parameter> ReadParameters(JsonLink jsonDevicesLink, JsonLink jsonDeviceLink, JsonChannel jsonChannel)
    {
        return jsonChannel.Links.Where(l => l.Rel == "parameter")
            .Select(l =>
            {
                var jsonParameterPath = Path.Combine(
                    this.RootPath, "devices",
                    jsonDevicesLink.Href, "channels",
                    jsonDeviceLink.Href, "parameters",
                    l.Href, "parameter.json"
                );

                var jsonParameter = JsonSerializer.Deserialize<JsonParameter>(File.ReadAllText(jsonParameterPath))
                    ?? throw new InvalidOperationException($"could not read parameter from path '{jsonParameterPath}'");

                return new Parameter(jsonParameter);
            })
            .ToList();
    }

    private static List<string> ReadRooms(JsonChannel jsonChannel)
        => jsonChannel.Links.Where(l => l.Rel == "room")
            .Select(l => l.Href.Replace("/room/", string.Empty))
            .ToList();
}
