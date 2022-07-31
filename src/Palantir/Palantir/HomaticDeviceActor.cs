using Proto;

namespace Palantir
{
    public class HomaticDeviceActor : IActor
    {
        private readonly string id;
        private readonly HomaticHttpClient homaticClient;
        private readonly ILogger<HomaticActor> logger;

        public HomaticDeviceActor(
            string id,
            HomaticHttpClient homaticClient,
            ILogger<HomaticActor> logger)
        {
            this.id = id ?? throw new ArgumentNullException(nameof(id));
            this.homaticClient = homaticClient ?? throw new ArgumentNullException(nameof(homaticClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
                logger.LogInformation("{type} ({pid}) has started", GetType(), context.Self);

                var device = await homaticClient.GetDeviceAsync(id);
            }
            if (context.Message is Stopped)
            {
                logger.LogInformation("{type} ({pid}) has started", GetType(), context.Self);
            }
        }
    }
}
