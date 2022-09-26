using Palantir.Apartment;
using Palantir.Homatic.Actors;
using Proto;
using Proto.DependencyInjection;

namespace Palantir.Sys;

public class ActorSystemService : IHostedService
{
    private readonly ActorSystem actorSystem;
    private readonly ILogger<ActorSystemService> logger;

    public ActorSystemService(ActorSystem actorSystem, ILogger<ActorSystemService> logger)
    {
        this.actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var props = this.actorSystem.DI().PropsFor<RootActor>();
        var root = this.actorSystem.Root.Spawn(props);
        this.logger.LogInformation("spawned {type}", typeof(RootActor));

        var homaticProps = this.actorSystem.DI().PropsFor<HomaticActor>(root);
        this.actorSystem.Root.Spawn(homaticProps);
        this.logger.LogInformation("spawned {type}", typeof(HomaticActor));

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await this.actorSystem.ShutdownAsync()
            .ConfigureAwait(false);
    }
}