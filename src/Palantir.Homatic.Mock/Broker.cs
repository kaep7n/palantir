using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Server;
using System.Text.Json;

namespace Palantir.Homatic.Mock;

public sealed class Broker : BackgroundService
{
    private readonly MqttServer server;
    private readonly IOptionsMonitor<HomaticOptions> optionsMonitor;
    private readonly ILogger<Broker> logger;
    private readonly List<Parameter> parameters = new();
    private readonly List<string> relevantIdentifiers = new()
    {
        "ACTUAL_TEMPERATURE",
        "SET_TEMPERATURE"
    };

    public Broker(IOptionsMonitor<HomaticOptions> optionsMonitor, ILogger<Broker> logger)
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

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("starting broker");

        this.logger.LogInformation("reading parameters from {homaticRootPath}", this.optionsMonitor.CurrentValue.RootPath);
        var parameterFiles = Directory.EnumerateFiles(this.optionsMonitor.CurrentValue.RootPath!, "parameter.json", SearchOption.AllDirectories);
        this.logger.LogInformation("read {count} parameters from {homaticRootPath}", parameterFiles.Count(), this.optionsMonitor.CurrentValue.RootPath);

        foreach (var parameterFile in parameterFiles)
        {
            if (!File.Exists(parameterFile))
                continue;

            var json = File.ReadAllText(parameterFile);

            var parameter = JsonSerializer.Deserialize<Parameter>(json);

            if (parameter is null)
                continue;

            this.logger.LogInformation("adding parameter {title} with topic {topic}", parameter.Title, parameter.MqttStatusTopic);

            this.parameters.Add(parameter);
        }

        this.logger.LogInformation("found {count} parameters", this.parameters.Count);

        this.logger.LogInformation("starting mqqt broker");
        await this.server.StartAsync();

        await base.StartAsync(cancellationToken);
        this.logger.LogInformation("started broker");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("stopping broker");

        this.logger.LogInformation("stopping mqqt broker");
        await this.server.StopAsync();
        this.logger.LogInformation("stopped mqqt broker");

        await base.StopAsync(cancellationToken);

        this.logger.LogInformation("stopped broker");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var relevantParameters = this.parameters
                .Where(p => this.relevantIdentifiers.Contains(p.Identifier))
                .ToList();

            var actualParameter = relevantParameters[Random.Shared.Next(0, relevantParameters.Count - 1)];

            var minimum = actualParameter.Minimum.GetDouble();
            var maximum = actualParameter.Maximum.GetDouble();

            var value = Math.Round(Random.Shared.NextDouble() * (maximum - minimum) + minimum, 2);

            var payload = new VeapMessage(DateTimeOffset.Now.ToUnixTimeMilliseconds(), value, 0);
            var serializedPayload = JsonSerializer.Serialize(payload);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(actualParameter.MqttStatusTopic)
                .WithPayload(serializedPayload)
                .Build();

            this.logger.LogInformation("publishing message {@payload} to topic {topic}", payload, actualParameter.MqttStatusTopic);

            await this.server.InjectApplicationMessage(new InjectedMqttApplicationMessage(message)
            {
                SenderClientId = "Mock"
            }, stoppingToken);

            await Task.Delay(1000, stoppingToken);
        }
    }

    public override void Dispose()
    {
        this.server.Dispose();
    }
}
