using Microsoft.Extensions.Logging;
using Proto;
using Proto.DependencyInjection;
using Proto.Router;
using System.Threading.Tasks;

namespace Palantir
{
    public class PersistorGroup : IActor
    {
        private readonly ILogger<PersistorGroup> logger;

        public PersistorGroup(ILogger<PersistorGroup> logger)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
                this.logger.LogInformation("starting new round robin group for {actor-type} with size {actor-pool-size}", typeof(Persistor), 5);
                var persistorProps = context.System.DI().PropsFor<Persistor>();
                var poolProps = context.NewRoundRobinPool(persistorProps, 5);
                var pid = context.Spawn(poolProps);

                this.logger.LogInformation("subscribing to message {device-data-type}", typeof(DeviceData));
                context.System.EventStream.Subscribe<DeviceData>(msg =>
                {
                    this.logger.LogDebug("sending message to pool {pool}", pid);
                    context.Send(pid, msg);
                });
            }

            return Task.CompletedTask;
        }
    }
}
