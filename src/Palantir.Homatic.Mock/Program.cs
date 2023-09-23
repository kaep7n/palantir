using Microsoft.AspNetCore.Mvc;
using Palantir.Homatic.Mock;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Host.UseSerilog((context, configuration)
    => configuration.ReadFrom.Configuration(context.Configuration.GetSection("Logging"))
);

builder.Services.Configure<HomaticOptions>(builder.Configuration.GetSection("Homatic"));

builder.Services.AddHostedService<Homatic>();

var app = builder.Build();

app.MapGet("device", (Homatic homatic)
    => homatic.GetRaw()
);

app.MapGet("device/{deviceId}", (Homatic homatic, string deviceId)
    => homatic.GetDevice(deviceId).GetRaw()
);

app.MapGet("device/{deviceId}/{channelId}", (Homatic homatic, string deviceId, string channelId)
    => homatic.GetChannel(deviceId, channelId).GetRaw()
);

app.MapGet("device/{deviceId}/{channelId}/{parameterId}", (Homatic homatic, string deviceId, string channelId, string parameterId)
    => homatic.GetParameter(deviceId, channelId, parameterId).GetRaw()
);

app.MapGet("device/{deviceId}/{channelId}/{parameterId}/~pv", (Homatic homatic, string deviceId, string channelId, string parameterId) =>
{
    return homatic.GetParameterValue(deviceId, channelId, parameterId);
});

app.MapPut("device/{deviceId}/{channelId}/{parameterId}/~pv", (Homatic homatic, string deviceId, string channelId, string parameterId, [FromBody] SetValueRequest request) =>
{
    homatic.SetParameterValue(deviceId, channelId, parameterId, request.Value);
});

app.Run();

public record SetValueRequest(
    [property: JsonPropertyName("v")] object Value
);