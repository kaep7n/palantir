using Microsoft.Extensions.Logging;
using Proto;
using Proto.DependencyInjection;

namespace Palantir.Homatic.Actors;

public class ChannelActor(PID apiPool, string id, ILogger<ChannelActor> logger) : BaseActor(apiPool, id, logger)
{
    private readonly Dictionary<string, PID> parameters = new();

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

    private Task OnGetChannelResult(IContext context, GetChannelResult result)
    {
        var homaticRooms = result.Channel.Links.Where(l => l.Rel == "room");

        foreach (var homaticRoom in homaticRooms)
        {
            //var homaticRoomId = homaticRoom.Href.Replace("/room/", string.Empty);

            //var room = context.Cluster().GetRoomGrain(Rooms.GetClusterIdentity(homaticRoomId));

            //var joinRoom = new JoinRoom(this.deviceId, this.id, homaticRoom.Href.Replace("/room/", string.Empty));
            //var roomJoined = await room.Join(joinRoom, CancellationToken.None);
        }

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
