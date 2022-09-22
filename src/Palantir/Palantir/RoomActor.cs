using Proto;

namespace Palantir
{
    public class RoomActor : IActor
    {
        private readonly ILogger<RoomActor> logger;
        private readonly string name;

        public RoomActor(ILogger<RoomActor> logger, string name)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            }

            return Task.CompletedTask;
        }
    }
}
