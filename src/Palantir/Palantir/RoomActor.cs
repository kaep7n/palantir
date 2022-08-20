using Proto;

namespace Palantir
{
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
                this.logger.LogInformation("room {id} ({name}) started", this.id, this.name);
            }
            if (context.Message is JoinRoom joinRoom)
            {
                this.logger.LogInformation("room {id} ({name}) has a new device {deviceId} with channel {channel}", this.id, this.name, joinRoom.DeviceId, joinRoom.ChannelId);

            }

            return Task.CompletedTask;
        }
    }

    public class DeviceActor : IActor
    {
        public Task ReceiveAsync(IContext context)
        {
            throw new NotImplementedException();
        }
    }
}
