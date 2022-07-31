using Proto;

namespace Palantir
{
    public class HomaticDeviceChannelParameterActor : IActor
    {
        private readonly string deviceId;
        private readonly string channelId;
        private readonly string id;
        private readonly HomaticHttpClient homaticClient;
        private readonly ILogger<HomaticActor> logger;

        public HomaticDeviceChannelParameterActor(
            string deviceId,
            string channelId,
            string id,
            HomaticHttpClient homaticClient,
            ILogger<HomaticActor> logger)
        {
            this.deviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
            this.channelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
            this.id = id ?? throw new ArgumentNullException(nameof(id));
            this.homaticClient = homaticClient ?? throw new ArgumentNullException(nameof(homaticClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
                logger.LogInformation("{type} ({pid}) has started", GetType(), context.Self);

                var parameter = await homaticClient.GetParameterAsync(deviceId, channelId, id);
            }
            if (context.Message is Stopped)
            {
                logger.LogInformation("{type} ({pid}) has started", GetType(), context.Self);
            }
        }
    }
}
