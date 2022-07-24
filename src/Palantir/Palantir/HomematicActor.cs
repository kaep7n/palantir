using Microsoft.Extensions.Options;
using Proto;

namespace Palantir
{
    public class HomaticOptions
    {
        public string Url { get; set; } = string.Empty;
    }

    public class HomaticActor : IActor
    {
        private readonly HomaticHttpClient homaticClient;
        private readonly IOptionsMonitor<HomaticOptions> optionsMonitor;
        private readonly ILogger<HomaticActor> logger;

        public HomaticActor(HomaticHttpClient homaticClient, IOptionsMonitor<HomaticOptions> optionsMonitor, ILogger<HomaticActor> logger)
        {
            this.homaticClient = homaticClient ?? throw new ArgumentNullException(nameof(homaticClient));
            this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
                logger.LogInformation("{type} ({pid}) has started", GetType(), context.Self);

                var devices = await homaticClient.GetDevicesAsync();
            }
            if (context.Message is Stopped)
            {
                logger.LogInformation("{type} ({pid}) has started", GetType(), context.Self);
            }
        }
    }
}
