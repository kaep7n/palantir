using Proto;
using Proto.Cluster;

namespace Palantir.Api;

public class ActorSystemService(ActorSystem actorSystem, ILogger<ActorSystemService> logger) : IHostedService
{
    private readonly ActorSystem actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
    private readonly ILogger<ActorSystemService> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await this.actorSystem.Cluster()
             .StartMemberAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await this.actorSystem
            .Cluster()
            .ShutdownAsync()
            .ConfigureAwait(false);
    }
}