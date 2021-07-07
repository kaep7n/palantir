using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Palantir
{
    public class PersistorService : IHostedService
    {
        private readonly ActorSystem actorSystem;
        private readonly ILogger<PersistorService> logger;
        private PID persistorGroup;

        public PersistorService(ActorSystem actorSystem, ILogger<PersistorService> logger)
        {
            this.actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.persistorGroup = this.actorSystem.Root.Spawn(
                this.actorSystem.DI().PropsFor<PersistorGroup>()
            );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
