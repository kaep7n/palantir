using Microsoft.Extensions.Logging;
using Proto;
using Proto.DependencyInjection;

namespace Palantir;

public class RootActor : IActor
{
    private readonly ILogger<RootActor> logger;
    private PID homatic = new();

    private readonly List<Room> rooms = new();

    public RootActor(ILogger<RootActor> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task ReceiveAsync(IContext context)
    {
        if (context.Message is Started)
        {
            this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);

            var rooms = new Dictionary<string, string>
            {
                { "1230", "Esszimmer" },
                { "1226", "Küche" },
                { "1228", "Leon" },
                { "1229", "Linus" },
                { "1227", "Schlafzimmer" },
                { "1225", "Wohnzimmer" },
                { "1231", "Bad" }
            };

            foreach (var room in rooms)
            {
                var roomProps = context.System.DI().PropsFor<RoomActor>(room.Key, room.Value);
                var roomActor = context.Spawn(roomProps);
                this.rooms.Add(new Room(roomActor, room.Key, room.Value));
            }
        }
        if (context.Message is Join joinRoom)
        {
            var room = this.rooms.FirstOrDefault(r => r.Id == joinRoom.Room);

            if (room is null)
            {
                this.logger.LogWarning("Room with id '{roomId}' not found.", joinRoom.Room);
                return Task.CompletedTask;
            }

            context.Forward(room.ActorId);
        }
        if (context.Message is Stopped)
        {
            this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);
        }

        return Task.CompletedTask;
    }
}

