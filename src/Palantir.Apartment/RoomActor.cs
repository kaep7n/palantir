using Microsoft.Extensions.Logging;
using Proto;

namespace Palantir.Apartment;

public class RoomActor : IActor
{
    private readonly ILogger<RoomActor> logger;
    private readonly string id;
    private readonly string name;

    public RoomActor(ILogger<RoomActor> logger, string id, string name)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.id = id ?? throw new ArgumentNullException(nameof(id));
        this.name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public Task ReceiveAsync(IContext context)
    {
        if (context.Message is Started)
        {
            this.logger.LogInformation("room {name} started", this.name);
        }
        if (context.Message is JoinRoom joinRoom)
        {
            this.logger.LogInformation("room {name} has a new device {deviceId} with channel {channel}", this.name, joinRoom.DeviceId, joinRoom.ChannelId);
            context.Respond(new RoomJoined(context.Self));
        }
        if (context.Message is ValueChanged valueChanged)
        {
            this.logger.LogInformation("room {name} received new value for device {target}: {value}", this.name, valueChanged.Target, valueChanged.Value);
        }

        return Task.CompletedTask;
    }
}
