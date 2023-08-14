using Microsoft.Extensions.Options;
using Palantir.Homatic.Mock;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Host.UseSerilog((context, configuration)
    => configuration.ReadFrom.Configuration(context.Configuration.GetSection("Logging"))
);

builder.Services.Configure<HomaticOptions>(builder.Configuration.GetSection("Homatic"));
builder.Services.AddHostedService<Broker>();

var app = builder.Build();

app.MapGet("device", (IOptionsSnapshot<HomaticOptions> options) =>
    Results.Content(
        File.ReadAllText(
            Path.Combine(options.Value.RootPath!, "devices.json")
            ),
        "application/json"
        )
    );

app.MapGet("device/{deviceId}", (IOptionsSnapshot<HomaticOptions> options, string deviceId) =>
    Results.Content(
        File.ReadAllText(
            Path.Combine(options.Value.RootPath!, @$"devices\{deviceId}\device.json")
            ),
        "application/json"
        )
    );

app.MapGet("device/{deviceId}/{channelId}", (IOptionsSnapshot<HomaticOptions> options, string deviceId, string channelId) =>
    Results.Content(
        File.ReadAllText(
            Path.Combine(options.Value.RootPath!, @$"devices\{deviceId}\channels\{channelId}\channel.json")
            ),
        "application/json"
        )
    );

app.MapGet("device/{deviceId}/{channelId}/{parameterId}", (IOptionsSnapshot<HomaticOptions> options, string deviceId, string channelId, string parameterId) =>
    Results.Content(
        File.ReadAllText(
            Path.Combine(options.Value.RootPath!, @$"devices\{deviceId}\channels\{channelId}\parameters\{parameterId}\parameter.json")
            ),
        "application/json"
        )
    );

app.MapGet("device/{deviceId}/{channelId}/{parameterId}/~pv", (IOptionsSnapshot<HomaticOptions> options, string deviceId, string channelId, string parameterId) =>
{
    var json = File.ReadAllText(
        Path.Combine(options.Value.RootPath!, @$"devices\{deviceId}\channels\{channelId}\parameters\{parameterId}\parameter.json")
        );

    var parameter = JsonSerializer.Deserialize<Parameter>(json);

    if (parameter is null)
        return Results.NotFound($"parameter '{deviceId}/{channelId}/{parameter}' not found.");

    var minimum = parameter.Minimum.GetDouble();
    var maximum = parameter.Maximum.GetDouble();

    var value = Math.Round(Random.Shared.NextDouble() * (maximum - minimum) + minimum, 2);

    var payload = new VeapMessage(DateTimeOffset.Now.ToUnixTimeMilliseconds(), value, 0);

    return Results.Json(payload);
});

app.Run();

