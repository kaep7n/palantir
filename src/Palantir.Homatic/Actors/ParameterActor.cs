using Microsoft.Extensions.Logging;
using Proto;

namespace Palantir.Homatic.Actors;

public class ParameterActor : IActor
{
    private readonly PID apiPool;
    private readonly string deviceId;
    private readonly string channelId;
    private readonly string id;

    private readonly ILogger<ParameterActor> logger;

    private Parameter parameter;
    private object currentValue;

    public ParameterActor(
        PID apiPool,
        string deviceId,
        string channelId,
        string id,
        ILogger<ParameterActor> logger)
    {
        this.apiPool = apiPool ?? throw new ArgumentNullException(nameof(apiPool));
        this.deviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
        this.channelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
        this.id = id ?? throw new ArgumentNullException(nameof(id));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task ReceiveAsync(IContext context)
    {
        if (context.Message is Started)
        {
            context.Request(this.apiPool, new GetParameter(this.deviceId, this.channelId, this.id), context.Self);
        }
        else if (context.Message is GetParameterResult result)
        {
            this.parameter = result.Parameter;
        }
        else if (context.Message is ParameterValueChanged pvc)
        {
            if (this.currentValue is null || !this.currentValue.Equals(pvc.Value))
            {
                this.logger.LogInformation(
                    "{deviceId}/{channelId}/{parameter} value has changed from '{currentValue}' to '{newValue}'",
                    this.deviceId,
                    this.channelId,
                    this.id,
                    this.currentValue,
                    pvc.Value
                );

                this.currentValue = pvc.Value;
            }
        }
        else if (context.Message is Stopped)
        {
            this.logger.LogDebug("{type} ({pid}) has stopped", this.GetType(), context.Self);
        }

        return Task.CompletedTask;
    }
}
