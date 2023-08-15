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

    private record ParameterGroup(string Id, string Identifier, string Type, string Unit, ParameterValue Min, ParameterValue Max);

    private record ParameterValue(string Kind, string Value);

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("starting broker");

        this.logger.LogInformation("reading parameters from {homaticRootPath}", this.optionsMonitor.CurrentValue.RootPath);
        var parameterFiles = Directory.EnumerateFiles(this.optionsMonitor.CurrentValue.RootPath!, "parameter.json", SearchOption.AllDirectories);
        this.logger.LogInformation("read {count} parameters from {homaticRootPath}", parameterFiles.Count(), this.optionsMonitor.CurrentValue.RootPath);

        var httpClient = new HttpClient();

        foreach (var parameterFile in parameterFiles)
        {
            if (!File.Exists(parameterFile))
                continue;

            var json = File.ReadAllText(parameterFile);

            var parameter = JsonSerializer.Deserialize<Parameter>(json);

            if (parameter is null)
                continue;

            //if (parameter.MqttStatusTopic is not null)
            //{
            //    var status = await httpClient.GetAsync($"http://192.168.2.101:2121/{parameter.MqttStatusTopic.Replace("status/", string.Empty)}/~pv");
            //    var result = await status.Content.ReadFromJsonAsync<VeapMessage>();

            //    var valueKind = result.Value.ValueKind;

            //    string value = string.Empty;

            //    try
            //    {
            //        value = result.Value.GetRawText();
            //    }
            //    catch (Exception ex)
            //    {
            //        this.logger.LogError(ex, "ERROR");
            //    }

            //    File.AppendAllText("params.csv", $"{parameter.Identifier};{parameter.MqttStatusTopic};{parameter.Minimum.ValueKind};{parameter.Minimum.GetRawText()};{parameter.Maximum.ValueKind};{parameter.Maximum.GetRawText()};{parameter.Type};{parameter.Unit};{valueKind};{value};{Environment.NewLine}");
            //}

            this.logger.LogInformation("adding parameter {title} with topic {topic}", parameter.Title, parameter.MqttStatusTopic);

            this.parameters.Add(parameter);
        }

        //var g = this.parameters
        //    .Where(p => p.Identifier != "$MASTER")
        //    .Select(p => new ParameterGroup(p.Id, p.Identifier, p.Type, p.Unit, new ParameterValue(p.Minimum.ValueKind.ToString(), p.Minimum.GetRawText()), new ParameterValue(p.Maximum.ValueKind.ToString(), p.Maximum.GetRawText())))
        //    .Distinct()
        //    .ToList();

        //var units = string.Join(Environment.NewLine, g.Select(i => i.Unit).Distinct().ToList());
        //var types = string.Join(Environment.NewLine, g.Select(i => i.Type).Distinct().ToList());
        //var identifiers = string.Join(Environment.NewLine, g.Select(i => i.Identifier).Distinct().ToList());
        //var minimum = string.Join(Environment.NewLine, g.Select(i => new { i.Min, i.Max }).Distinct().ToList());

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

            var parameter = relevantParameters[Random.Shared.Next(0, relevantParameters.Count - 1)];

            var payload = Randomizer.VeapMessage(parameter);

            var serializedPayload = JsonSerializer.Serialize(payload);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(parameter.MqttStatusTopic)
                .WithPayload(serializedPayload)
                .Build();

            this.logger.LogInformation("publishing message {@payload} to topic {topic}", payload, parameter.MqttStatusTopic);

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
