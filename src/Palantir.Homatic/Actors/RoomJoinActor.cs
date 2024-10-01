using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;

namespace Palantir.Homatic.Actors;

public record JoinPalantirRoom(string DeviceId, string[] Rooms);

public record PalantirRoomJoined(RoomDefinition Room);

public class RoomJoinActor : IActor
{
    private readonly string[] rooms;
    private readonly ILogger<BaseActor> logger;

    public RoomJoinActor(string[] rooms, ILogger<BaseActor> logger)
    {
        this.rooms = rooms ?? throw new ArgumentNullException(nameof(rooms));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task ReceiveAsync(IContext context)
                => context.Message switch
                {
                    Started => this.OnStarted(context),
                    JoinPalantirRoom joinRoom => this.OnJoinRoom(context, joinRoom),
                    Stopped => this.OnStopped(context),
                    _ => Task.CompletedTask
                };

    private Task OnStarted(IContext context)
    {
        this.logger.LogInformation("{type} ({pid}) has started", this.GetType(), context.Self);

        return Task.CompletedTask;
    }

    private Task OnStopped(IContext context)
    {
        this.logger.LogInformation("{type} ({pid}) has stopped", this.GetType(), context.Self);

        return Task.CompletedTask;
    }

    private async Task OnJoinRoom(IContext context, JoinPalantirRoom joinPalantirRoom)
    {
        foreach (var homaticRoomid in joinPalantirRoom.Rooms)
        {
            if (context.Parent is null)
                continue;

            var roomId = homaticRoomid switch
            {
                "1230" => "dining_room",
                "1226" => "kitchen",
                "1228" => "nursery_1",
                "1229" => "nursery_2",
                "1227" => "bedroom",
                "1225" => "living_room",
                "1231" => "bathroom",
                _ => throw new ArgumentOutOfRangeException($"Unexpected room Homatic room id '{homaticRoomid}' unable to map it to an actual room.")
            };

            var room = context.Cluster().GetRoomGrain(roomId);

            var joinRoom = new JoinRoom(joinPalantirRoom.DeviceId, context.Self);

            var roomJoined = await room.Join(joinRoom, CancellationToken.None);

            context.Send(context.Parent, roomJoined);
        }

        context.Stop(context.Self);
    }
}
