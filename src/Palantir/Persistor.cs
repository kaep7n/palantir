using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Threading.Tasks;
using Palantir.Homatic;

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
            try
            {
                if (context.Message is Started)
                {
                    this.logger.LogInformation("persistor started");
                }

                if (context.Message is DeviceParameterValue msg)
                {
                    this.logger.LogDebug("received message {message}", msg);

                    var response = await this.client.Client.IndexAsync(msg, idx => idx.Index($"homatic-{msg.Parameter.ToLower()}-{msg.Timestamp:yyyy-MM}")).ConfigureAwait(false);

                    this.logger.LogInformation("elastic response: {response}", response);
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, "unexpected error while persisting parameter data");
            }
        }
    }
}
