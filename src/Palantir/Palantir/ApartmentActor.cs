using Proto;
using Proto.DependencyInjection;

namespace Palantir;

public class ApartmentActor : IActor
{
    private readonly ILogger<ApartmentActor> logger;
    private PID homatic = new();

    private readonly List<Room> rooms = new();

    public ApartmentActor(ILogger<ApartmentActor> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task ReceiveAsync(IContext context)
    {
        if (context.Message is Started)
        {
            logger.LogDebug("{type} ({pid}) has started", GetType(), context.Self);

            var rooms = new List<string>
            {
                "Esszimmer",
                "Küche",
                "Leon",
                "Linus",
                "Schlafzimmer",
                "Wohnzimmer",
                "Bad"
            };

            foreach (var room in rooms)
            {
                var roomProps = context.System.DI().PropsFor<RoomActor>(room);
                var roomActor = context.Spawn(roomProps);
                this.rooms.Add(new Room(roomActor, room));
            }

            var props = context.System.DI().PropsFor<HomaticActor>(this.rooms);
            homatic = context.Spawn(props);
        }
        if (context.Message is Stopped)
        {
            logger.LogDebug("{type} ({pid}) has started", GetType(), context.Self);
        }

        return Task.CompletedTask;
    }
}

