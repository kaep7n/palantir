using Microsoft.AspNetCore.Mvc;
using Palantir.Homatic.Mock;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Host.UseSerilog((context, configuration)
    => configuration.ReadFrom.Configuration(context.Configuration.GetSection("Logging"))
);

builder.Services.Configure<HomaticOptions>(builder.Configuration.GetSection("Homatic"));

builder.Services.AddSingleton<Homatic>();
builder.Services.AddHostedService(p => p.GetRequiredService<Homatic>());

var app = builder.Build();

app.MapGet("device", ([FromServices] Homatic homatic)
    => homatic.GetRaw()
);

app.MapGet("device/{deviceId}", ([FromServices] Homatic homatic, string deviceId)
    => homatic.GetDevice(deviceId).GetRaw()
);

app.MapGet("device/{deviceId}/{channelId}", ([FromServices] Homatic homatic, string deviceId, string channelId)
    =>
{
    var channel = homatic.GetChannel(deviceId, channelId).GetRaw();

    return channel;
}
);

app.MapGet("device/{deviceId}/{channelId}/{parameterId}", ([FromServices] Homatic homatic, string deviceId, string channelId, string parameterId)
    => homatic.GetParameter(deviceId, channelId, parameterId).GetRaw()
);

app.MapGet("device/{deviceId}/{channelId}/{parameterId}/~pv", ([FromServices] Homatic homatic, string deviceId, string channelId, string parameterId) =>
{
    return homatic.GetParameterValue(deviceId, channelId, parameterId);
});

app.MapPut("device/{deviceId}/{channelId}/{parameterId}/~pv", ([FromServices] Homatic homatic, string deviceId, string channelId, string parameterId, [FromBody] SetValueRequest request) =>
{
    homatic.SetParameterValue(deviceId, channelId, parameterId, request.Value);
});

app.Run();

public record SetValueRequest(
    [property: JsonPropertyName("v")] object Value
);