using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Server;
using System.Text.Json;

namespace Palantir.Homatic.Mock;

public sealed class Mock : IHostedService
{
    private readonly MqttServer server;
    private readonly IOptionsMonitor<HomaticOptions> optionsMonitor;
    private readonly ILogger<Mock> logger;

    public Mock(IOptionsMonitor<HomaticOptions> optionsMonitor, ILogger<Mock> logger)
    {
        var mqttFactory = new MqttFactory();
        var mqttServerOptions = mqttFactory
            .CreateServerOptionsBuilder()
            .WithDefaultEndpoint()
            .Build();

        this.server = mqttFactory.CreateMqttServer(mqttServerOptions);
        this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public FakeHomatic? Homatic { get; set; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("starting broker");

        var rootPath = this.optionsMonitor.CurrentValue.RootPath;
        var jsonDevicesPath = Path.Combine(rootPath, "devices.json");
        var jsonDevices = JsonSerializer.Deserialize<JsonDevices>(
            File.ReadAllText(jsonDevicesPath)
        );

        var devices = new List<FakeDevice>();

        foreach (var jsonDevicesLink in jsonDevices.Links)
        {
            if (jsonDevicesLink.Rel != "device")
                continue;

            var jsonDevicePath = Path.Combine(rootPath, "devices", jsonDevicesLink.Href, "device.json");
            var jsonDevice = JsonSerializer.Deserialize<JsonDevice>(File.ReadAllText(jsonDevicePath));

            var channels = new List<FakeChannel>();

            foreach (var jsonDeviceLink in jsonDevice.Links)
            {
                if (jsonDeviceLink.Rel != "channel")
                    continue;

                var jsonChannelPath = Path.Combine(rootPath, "devices", jsonDevicesLink.Href, "channels", jsonDeviceLink.Href, "channel.json");
                var jsonChannel = JsonSerializer.Deserialize<JsonChannel>(File.ReadAllText(jsonChannelPath));

                var rooms = new List<string>();
                var parameters = new List<FakeParameter>();

                foreach (var jsonChannelLink in jsonChannel.Links)
                {
                    if (jsonChannelLink.Rel != "parameter" && jsonChannelLink.Rel != "room")
                        continue;

                    if (jsonChannelLink.Rel == "room")
                    {
                        rooms.Add(jsonChannelLink.Href.Replace("/room/", string.Empty));
                        logger.LogInformation("Adding room {room}", jsonChannelLink.Href.Replace("/room/", string.Empty));
                    }

                    if (jsonChannelLink.Rel == "parameter")
                    {
                        var jsonParameterPath = Path.Combine(rootPath, "devices", jsonDevicesLink.Href, "channels", jsonDeviceLink.Href, "parameters", jsonChannelLink.Href, "parameter.json");
                        var jsonParameter = JsonSerializer.Deserialize<JsonParameter>(File.ReadAllText(jsonParameterPath));

                        parameters.Add(new FakeParameter(jsonParameter));
                        logger.LogInformation("Adding parameter {parameter}", jsonParameter.Identifier);
                    }
                }

                channels.Add(new FakeChannel(jsonChannel, rooms, parameters));
                logger.LogInformation("Adding channel {channel}", jsonChannel.Identifier);
            }

            devices.Add(new FakeDevice(jsonDevice, channels));
            logger.LogInformation("Adding device {device}", jsonDevice.Identifier);
        }

        this.Homatic = new FakeHomatic(jsonDevices, devices);

        this.logger.LogInformation("initialized homatic fake");

        this.logger.LogInformation("starting mqqt broker");
        await this.server.StartAsync();

        this.logger.LogInformation("started broker");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("stopping broker");

        this.logger.LogInformation("stopping mqqt broker");
        await this.server.StopAsync();
        this.logger.LogInformation("stopped mqqt broker");

        this.logger.LogInformation("stopped broker");
    }


    //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //{
    //    while (!stoppingToken.IsCancellationRequested)
    //    {
    //        //var relevantParameters = this.parameters
    //        //    .Where(p => this.relevantIdentifiers.Contains(p.Identifier))
    //        //    .ToList();

    //        //var parameter = relevantParameters[Random.Shared.Next(0, relevantParameters.Count - 1)];

    //        //var payload = Randomizer.VeapMessage(parameter);

    //        //var serializedPayload = JsonSerializer.Serialize(payload);

    //        //var message = new MqttApplicationMessageBuilder()
    //        //    .WithTopic(parameter.MqttStatusTopic)
    //        //    .WithPayload(serializedPayload)
    //        //    .Build();

    //        //this.logger.LogInformation("publishing message {@payload} to topic {topic}", payload, parameter.MqttStatusTopic);

    //        //await this.server.InjectApplicationMessage(new InjectedMqttApplicationMessage(message)
    //        //{
    //        //    SenderClientId = "Mock"
    //        //}, stoppingToken);

    //        await Task.Delay(1000, stoppingToken);
    //    }
    //}

    public void Dispose()
    {
        this.server.Dispose();
    }
}
