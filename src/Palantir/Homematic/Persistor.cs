using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Threading.Tasks;

namespace Palantir
{
    public class Persistor : IActor
    {
        private readonly PersistorClient client;
        private readonly ILogger<Persistor> logger;

        public Persistor(PersistorClient client, ILogger<Persistor> logger)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
                this.logger.LogInformation("persistor started");
            }

            if (context.Message is DeviceData msg)
            {
                this.logger.LogDebug("received message {message}", msg);

                await this.client.Client.IndexAsync(msg, idx => idx.Index("homematic"));
            }

        }
    }
}
