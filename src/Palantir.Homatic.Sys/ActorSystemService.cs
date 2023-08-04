using Palantir.Homatic.Actors;
using Proto;
using Proto.Cluster;
using Proto.DependencyInjection;

namespace Palantir.Sys;

public class ActorSystemService(ActorSystem actorSystem, ILogger<ActorSystemService> logger) : IHostedService
{
    private readonly ActorSystem actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
    private readonly ILogger<ActorSystemService> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await this.actorSystem.Cluster()
             .StartMemberAsync();

        var homaticProps = this.actorSystem.DI().PropsFor<RootActor>();
        this.actorSystem.Root.Spawn(homaticProps);
        this.logger.LogInformation("spawned {type}", typeof(RootActor));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await this.actorSystem
            .Cluster()
            .ShutdownAsync()
            .ConfigureAwait(false);
    }
}