using Microsoft.Extensions.Logging;
using Palantir.Homatic.Http;
using Proto;

namespace Palantir.Homatic.Actors;

public class ApiActor : IActor
{
    private readonly HomaticHttpClient http;
    private readonly ILogger<ApiActor> logger;

    public ApiActor(HomaticHttpClient http, ILogger<ApiActor> logger)
    {
        this.http = http ?? throw new ArgumentNullException(nameof(http));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task ReceiveAsync(IContext context)
        => context.Message switch
        {
            Started => this.OnStarted(context),
            GetDevices => this.OnGetDevices(context),
            GetDevice msg => this.OnGetDevice(context, msg),
            GetChannel msg => this.OnGetChannel(context, msg),
            GetParameter msg => this.OnGetParameter(context, msg),
            Stopped => this.OnStopped(context),
            _ => Task.CompletedTask
        };

    private Task OnStopped(IContext context)
    {
        this.logger.LogDebug("Http receiver stopped from '{pid}'", context.Parent?.Id);
        return Task.CompletedTask;
    }

    private async Task OnGetParameter(IContext context, GetParameter getParameter)
    {
        var parameter = await this.http.GetParameterAsync(getParameter.DeviceId, getParameter.ChannelId, getParameter.Id)
            ?? throw new InvalidOperationException($"Unable to get parameter with id '{getParameter.Id}' on channel '{getParameter.ChannelId}' and device '{getParameter.DeviceId}' from Homatic.");

        context.Respond(new GetParameterResult(parameter));
    }

    private async Task OnGetChannel(IContext context, GetChannel getChannel)
    {
        var channel = await this.http.GetChannelAsync(getChannel.DeviceId, getChannel.Id)
                        ?? throw new InvalidOperationException($"Unable to get channel with id '{getChannel.Id}' on device '{getChannel.DeviceId}' from Homatic.");

        context.Respond(new GetChannelResult(channel));
    }

    private async Task OnGetDevice(IContext context, GetDevice getDevice)
    {
        var device = await this.http.GetDeviceAsync(getDevice.Id)
                        ?? throw new InvalidOperationException($"Unable to get device with id '{getDevice.Id}' from Homatic.");

        context.Respond(new GetDeviceResult(device));
    }

    private async Task OnGetDevices(IContext context)
    {
        var devices = await this.http.GetDevicesAsync()
                        ?? throw new InvalidOperationException("Unable to get devices from Homatic.");

        context.Respond(new GetDevicesResult(devices));
    }

    private Task OnStarted(IContext context)
    {
        this.logger.LogDebug("Http receiver started from '{pid}'", context.Parent?.Id);
        return Task.CompletedTask;
    }
}

public record GetDevices();

public record GetDevicesResult(Devices Devices);

public record GetDevice(string Id);

public record GetDeviceResult(Device Device);

public record GetChannel(string DeviceId, string Id);

public record GetChannelResult(Channel Channel);

public record GetParameter(string DeviceId, string ChannelId, string Id);

public record GetParameterResult(Parameter Parameter);