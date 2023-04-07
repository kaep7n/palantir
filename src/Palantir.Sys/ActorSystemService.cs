﻿using Palantir.Apartment;
using Palantir.Homatic.Actors;
using Proto;
using Proto.Cluster;
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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await this.actorSystem.Cluster()
             .StartMemberAsync();

        var props = this.actorSystem.DI().PropsFor<RootActor>();
        var root = this.actorSystem.Root.Spawn(props);
        this.logger.LogInformation("spawned {type}", typeof(RootActor));

        var homaticProps = this.actorSystem.DI().PropsFor<HomaticActor>(root);
        this.actorSystem.Root.Spawn(homaticProps);
        this.logger.LogInformation("spawned {type}", typeof(HomaticActor));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await this.actorSystem
            .Cluster()
            .ShutdownAsync()
            .ConfigureAwait(false);
    }
}