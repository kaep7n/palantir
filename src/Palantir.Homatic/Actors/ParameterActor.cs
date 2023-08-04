using Microsoft.Extensions.Logging;
using Proto;

namespace Palantir.Homatic.Actors;

public class ParameterActor(PID apiPool, string id, ILogger<ParameterActor> logger) : BaseActor(apiPool, id, logger)
{
    private object? currentValue;

    public override Task ReceiveAsync(IContext context)
    {
        base.ReceiveAsync(context);

        return context.Message switch
        {
            GetParameterResult => Task.CompletedTask,
            ParameterValueChanged msg => this.OnParameterValueChanged(context, msg),
            _ => Task.CompletedTask
        };
    }

    private Task OnParameterValueChanged(IContext context, ParameterValueChanged pvc)
    {
        if (this.currentValue is null || !this.currentValue.Equals(pvc.Value))
        {
            this.logger.LogInformation(
                "{id} value has changed from '{currentValue}' to '{newValue}'",
                this.id,
                this.currentValue,
                pvc.Value
            );

            this.currentValue = pvc.Value;
        }

        return Task.CompletedTask;
    }
}
