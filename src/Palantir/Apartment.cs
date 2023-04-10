using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;

namespace Palantir;

public class Apartment : ApartmentGrainBase
{
    private readonly IContext context;
    private readonly ClusterIdentity clusterIdentity;
    private ILogger<Apartment> logger;

    public Apartment(IContext context, ClusterIdentity clusterIdentity, ILogger<Apartment> logger) : base(context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.clusterIdentity = clusterIdentity ?? throw new ArgumentNullException(nameof(clusterIdentity));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private Guid Id { get; } = Guid.NewGuid();

    public override Task OnStarted()
    {
        return base.OnStarted();
    }

    public override Task OnReceive()
    {
        return base.OnReceive();
    }

    public override Task<GetStateResponse> GetState(GetStateRequest request)
    {
        this.logger.LogInformation("{id}: Getting State for {source}", this.Id, request.Source);
        return Task.FromResult(new GetStateResponse());
    }
}
