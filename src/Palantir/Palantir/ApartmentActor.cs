using Proto;
using Proto.DependencyInjection;

namespace Palantir;

public class ApartmentActor : IActor
{
    private readonly ILogger<ApartmentActor> logger;
    private PID homatic = new();

    private readonly Dictionary<string, PID> rooms = new();

    public ApartmentActor(ILogger<ApartmentActor> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task ReceiveAsync(IContext context)
    {
        if (context.Message is Started)
        {
            logger.LogDebug("{type} ({pid}) has started", GetType(), context.Self);

            var rooms = new List<Tuple<string, string>>
            {
                new("1230", "Esszimmer"),
                new("1226", "Küche"),
                new("1228", "Leon"),
                new("1229", "Linus"),
                new("1227", "Schlafzimmer"),
                new("1225", "Wohnzimmer"),
                new("1231", "Bad")
            };

            foreach (var room in rooms)
            {
                var roomProps = context.System.DI().PropsFor<RoomActor>(room.Item1, room.Item2);
                var roomActor = context.Spawn(roomProps);
                this.rooms.Add(room.Item1, roomActor);
            }

            var props = context.System.DI().PropsFor<HomaticActor>();
            homatic = context.Spawn(props);
        }
        if (context.Message is JoinRoom joinRoom)
        {
            var room = this.rooms[joinRoom.Room];
            context.Forward(room);
        }
        if (context.Message is Stopped)
        {
            logger.LogDebug("{type} ({pid}) has started", GetType(), context.Self);
        }

        return Task.CompletedTask;
    }
}

