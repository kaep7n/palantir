using Microsoft.AspNetCore.Mvc;
using Palantir.Homatic.Mock;
using Serilog;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Host.UseSerilog((context, configuration)
    => configuration.ReadFrom.Configuration(context.Configuration.GetSection("Logging"))
);

builder.Services.Configure<HomaticOptions>(builder.Configuration.GetSection("Homatic"));
builder.Services.AddHostedService<Mock>();

var app = builder.Build();

app.MapGet("device", (Mock mock)
    => mock.Homatic?.GetRaw()
);

app.MapGet("device/{deviceId}", (Mock mock, string deviceId)
    => mock.Homatic?.GetDevice(deviceId).GetRaw()
);

app.MapGet("device/{deviceId}/{channelId}", (Mock mock, string deviceId, string channelId)
    => mock.Homatic?.GetChannel(deviceId, channelId).GetRaw()
);

app.MapGet("device/{deviceId}/{channelId}/{parameterId}", (Mock mock, string deviceId, string channelId, string parameterId)
    => mock.Homatic?.GetParameter(deviceId, channelId, parameterId).GetRaw()
);

app.MapGet("device/{deviceId}/{channelId}/{parameterId}/~pv", (Mock mock, string deviceId, string channelId, string parameterId) =>
    { }
);

app.MapPut("device/{deviceId}/{channelId}/{parameterId}/~pv", (FakeHomatic homatic, string deviceId, string channelId, string parameterId, [FromBody] SetValueRequest request) =>
{
    var parameter = homatic.Devices.FirstOrDefault(d => d.Identifier == deviceId)?
        .Channels.FirstOrDefault(c => c.Identifier == channelId)?
        .Parameters.FirstOrDefault(p => p.Identifier == parameterId);

    if (parameter is not null)
        return;
});

app.Run();

