﻿using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;
using Proto.DependencyInjection;

namespace Palantir.Homatic.Actors;

public class ChannelActor(PID apiPool, string id, ILogger<ChannelActor> logger) : BaseActor(apiPool, id, logger)
{
    private readonly Dictionary<string, PID> parameters = new();

    private RoomDefinition? roomDefinition;

    public override Task ReceiveAsync(IContext context)
    {
        base.ReceiveAsync(context);

        return context.Message switch
        {
            GetChannelResult msg => this.OnGetChannelResult(context, msg),
            ParameterValueChanged msg => this.OnParameterValueChanged(context, msg),
            PalantirRoomJoined msg => this.OnRoomJoined(msg),
            TemperatureChanged msg => this.InformRoomAsync(context, msg),
            SetTemperatureChanged msg => this.InformRoomAsync(context, msg),
            _ => Task.CompletedTask
        };
    }

    protected override Task OnStarted(IContext context)
    {
        context.Request(this.apiPool, new GetChannel(this.id), context.Self);

        return base.OnStarted(context);
    }

    private async Task InformRoomAsync(IContext context, object msg)
    {
        var room = context.Cluster().GetRoomGrain(this.roomDefinition!.Id);

        if (msg is TemperatureChanged temperatureChanged)
            await room.OnTemperatureChanged(temperatureChanged, CancellationToken.None);
        if (msg is SetTemperatureChanged setTemperatureChanged)
            await room.OnSetTemperatureChanged(setTemperatureChanged, CancellationToken.None);
    }

    private Task OnGetChannelResult(IContext context, GetChannelResult result)
    {
        var homaticRooms = result.Channel.Links.Where(l => l.Rel == "room")
            .Select(r => r.Href.Replace("/room/", string.Empty))
            .ToArray();

        var joinRoomProps = context.System.DI()
            .PropsFor<RoomJoinActor>();

        var roomJoinActor = context.Spawn(joinRoomProps);

        var joinRoomMessage = new JoinPalantirRoom(this.id, homaticRooms);
        context.Send(roomJoinActor, joinRoomMessage);

        foreach (var link in result.Channel.Links)
        {
            if (link.Href == ".." || link.Rel != "parameter")
                continue;

            var props = context.System.DI().PropsFor<ParameterActor>(this.apiPool, $"{this.id}/{link.Href}");
            var pid = context.Spawn(props);

            this.parameters.Add(link.Href, pid);
        }

        return Task.CompletedTask;
    }

    private Task OnRoomJoined(PalantirRoomJoined msg)
    {
        this.roomDefinition = msg.Room;

        return Task.CompletedTask;
    }

    private Task OnParameterValueChanged(IContext context, ParameterValueChanged pvc)
    {
        if (this.parameters.TryGetValue(pvc.Parameter, out var parameter))
        {
            context.Forward(parameter);
        }
        else
        {
            this.logger.LogWarning("could not find parameter '{parameter}' on channel '{channel}'", pvc.Parameter, this.id);
        }

        return Task.CompletedTask;
    }
}
