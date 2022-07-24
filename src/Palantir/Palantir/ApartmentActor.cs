using Proto;
using Proto.DependencyInjection;

namespace Palantir;

public class ApartmentActor : IActor
{
    private readonly ILogger<ApartmentActor> logger;
    private PID homatic = new();

    public ApartmentActor(ILogger<ApartmentActor> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task ReceiveAsync(IContext context)
    {
        if (context.Message is Started)
        {
            logger.LogInformation("{type} ({pid}) has started", GetType(), context.Self);

            var props = context.System.DI().PropsFor<HomaticActor>();
            homatic = context.Spawn(props);
        }
        if (context.Message is Stopped)
        {
            logger.LogInformation("{type} ({pid}) has started", GetType(), context.Self);
        }

        return Task.CompletedTask;
    }
}

