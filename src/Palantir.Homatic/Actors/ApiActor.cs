using Microsoft.Extensions.Logging;
using Palantir.Homatic.Http;
using Proto;
using System.Text.Json;

namespace Palantir.Homatic.Actors;

public class ApiActor(HomaticHttpClient http, ILogger<ApiActor> logger) : IActor
{
    private readonly HomaticHttpClient http = http ?? throw new ArgumentNullException(nameof(http));
    private readonly ILogger<ApiActor> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task ReceiveAsync(IContext context)
        => context.Message switch
        {
            Started => this.OnStarted(context),
            GetDevices => this.OnGetDevices(context),
            GetDevice msg => this.OnGetDevice(context, msg),
            GetChannel msg => this.OnGetChannel(context, msg),
            GetParameter msg => this.OnGetParameter(context, msg),
            GetParameterValue msg => this.OnGetParameterValue(context, msg),
            Stopped => this.OnStopped(context),
            _ => Task.CompletedTask
        };

    private Task OnStarted(IContext context)
    {
        this.logger.LogDebug("Http receiver started from '{pid}'", context.Parent?.Id);
        return Task.CompletedTask;
    }

    private Task OnStopped(IContext context)
    {
        this.logger.LogDebug("Http receiver stopped from '{pid}'", context.Parent?.Id);
        return Task.CompletedTask;
    }

    private async Task OnGetDevices(IContext context)
    {
        var devices = await this.http.GetDevicesAsync()
                        ?? throw new InvalidOperationException("Unable to get devices from Homatic.");

        context.Respond(new GetDevicesResult(devices));
    }

    private async Task OnGetDevice(IContext context, GetDevice getDevice)
    {
        var device = await this.http.GetDeviceAsync(getDevice.Id)
                        ?? throw new InvalidOperationException($"Unable to get device with id '{getDevice.Id}' from Homatic.");

        context.Respond(new GetDeviceResult(device));
    }

    private async Task OnGetChannel(IContext context, GetChannel getChannel)
    {
        var id = getChannel.Id.Split("/");

        var channel = await this.http.GetChannelAsync(id[0], id[1])
                        ?? throw new InvalidOperationException($"Unable to get channel with id '{getChannel.Id}' from Homatic.");

        context.Respond(new GetChannelResult(channel));
    }

    private async Task OnGetParameter(IContext context, GetParameter getParameter)
    {
        var id = getParameter.Id.Split("/");

        var parameter = await this.http.GetParameterAsync(id[0], id[1], id[2])
            ?? throw new InvalidOperationException($"Unable to get parameter with id '{getParameter.Id}' from Homatic.");

        context.Respond(new GetParameterResult(parameter));
    }

    private async Task OnGetParameterValue(IContext context, GetParameterValue getParameterValue)
    {
        var id = getParameterValue.Id.Split("/");

        var data = await this.http.GetParameterValueAsync(id[0], id[1], id[2])
            ?? throw new InvalidOperationException($"Unable to get parameter value with id '{getParameterValue.Id}' from Homatic.");

        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp);
        var value = data.Value.ValueKind switch
        {
            JsonValueKind.Number => data.Value.GetDecimal(),
            JsonValueKind.String => data.Value.GetString(),
            JsonValueKind.False => data.Value.GetBoolean(),
            JsonValueKind.True => (object)data.Value.GetBoolean(),
            _ => throw new ArgumentException($"Unexpected Value Kind {data.Value.ValueKind}")
        } ?? throw new InvalidOperationException($"unable to convert json value {data.Value} from type {data.Value.ValueKind}.");


        context.Respond(new GetParameterValueResult(timestamp, value));
    }
}

public record GetDevices();

public record GetDevicesResult(Devices Devices);

public record GetDevice(string Id);

public record GetDeviceResult(Device Device);

public record GetChannel(string Id);

public record GetChannelResult(Channel Channel);

public record GetParameter(string Id);

public record GetParameterResult(Parameter Parameter);

public record GetParameterValue(string Id);

public record GetParameterValueResult(DateTimeOffset Timestamp, object Value);