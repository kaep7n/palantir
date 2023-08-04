﻿using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;
using Proto.Cluster.PubSub;
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
            _ => Task.CompletedTask
        };
    }

    protected override Task OnStarted(IContext context)
    {
        context.Request(this.apiPool, new GetChannel(this.id), context.Self);

        return base.OnStarted(context);
    }

    private async Task OnGetChannelResult(IContext context, GetChannelResult result)
    {
        var homaticRooms = result.Channel.Links.Where(l => l.Rel == "room");

        foreach (var homaticRoom in homaticRooms)
        {
            var homaticRoomId = homaticRoom.Href.Replace("/room/", string.Empty);

            var roomId = homaticRoomId switch
            {
                "1230" => "dining_room",
                "1226" => "kitchen",
                "1228" => "nursery_1",
                "1229" => "nursery_2",
                "1227" => "bedroom",
                "1225" => "living_room",
                "1231" => "bathroom",
                _ => throw new ArgumentOutOfRangeException($"Unexpected room Homatic room id '{homaticRoomId}' unable to map it to an actual room.")
            };

            var room = context.Cluster().GetRoomGrain(roomId);

            var joinRoom = new JoinRoom(this.id, context.Self);

            var roomJoined = await room.Join(joinRoom, CancellationToken.None);

            this.roomDefinition = roomJoined?.Definition;
        }

        foreach (var link in result.Channel.Links)
        {
            if (link.Href == ".." || link.Rel != "parameter")
                continue;

            var props = context.System.DI().PropsFor<ParameterActor>(this.apiPool, $"{this.id}/{link.Href}");
            var pid = context.Spawn(props);

            this.parameters.Add(link.Href, pid);
        }
    }

    private async Task OnParameterValueChanged(IContext context, ParameterValueChanged pvc)
    {
        if (this.parameters.TryGetValue(pvc.Parameter, out var parameter))
        {
            context.Forward(parameter);

            if (this.roomDefinition is not null)
            {
                var pubisher = context.Cluster().Publisher();

                await pubisher.Publish($"rooms/{this.roomDefinition.Id}", new ValueChanged { Value = pvc.Value.ToString() });
            }
        }
        else
        {
            this.logger.LogWarning("could not find parameter '{parameter}' on channel '{channel}'", pvc.Parameter, this.id);
        }
    }
}
