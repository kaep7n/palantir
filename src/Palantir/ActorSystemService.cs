using Proto;
using Proto.DependencyInjection;

namespace Palantir;

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
        var props = actorSystem.DI().PropsFor<ApartmentActor>();
        actorSystem.Root.Spawn(props);
        this.logger.LogInformation("spawned {type}", typeof(ApartmentActor));

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await actorSystem.ShutdownAsync()
            .ConfigureAwait(false);
    }
}