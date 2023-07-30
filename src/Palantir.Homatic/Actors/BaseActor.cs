using Microsoft.Extensions.Logging;
using Proto;

namespace Palantir.Homatic.Actors;

public class BaseActor(PID apiPool, string id, ILogger<BaseActor> logger) : IActor
{
    protected readonly PID apiPool = apiPool ?? throw new ArgumentNullException(nameof(apiPool));
    protected readonly string id = id ?? throw new ArgumentNullException(nameof(id));
    protected readonly ILogger<BaseActor> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public virtual Task ReceiveAsync(IContext context)
        => context.Message switch
        {
            Started => this.OnStarted(context),
            Stopped => this.OnStopped(context),
            _ => Task.CompletedTask
        };

    protected virtual Task OnStarted(IContext context)
    {
        this.logger.LogInformation("{type} ({pid}) ({id}) has started", this.GetType(), context.Self, this.id);

        return Task.CompletedTask;
    }

    protected virtual Task OnStopped(IContext context)
    {
        this.logger.LogInformation("{type} ({pid}) ({id}) has stopped", this.GetType(), context.Self, this.id);

        return Task.CompletedTask;
    }
}
