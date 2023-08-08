using Microsoft.Extensions.Logging;
using Proto;

namespace Palantir.Homatic.Actors;

public class ParameterActor(PID apiPool, string id, ILogger<ParameterActor> logger) : BaseActor(apiPool, id, logger)
{
    private Parameter? parameter;
    private object? currentValue;


    public override Task ReceiveAsync(IContext context)
    {
        base.ReceiveAsync(context);

        return context.Message switch
        {
            GetParameterResult msg => OnGetParameterResult(msg),
            ParameterValueChanged msg => this.OnParameterValueChanged(context, msg),
            _ => Task.CompletedTask
        };
    }

    private Task OnGetParameterResult(GetParameterResult msg)
    {
        this.parameter = msg.Parameter;

        return Task.CompletedTask;
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

            if (parameter is null)
            {
                return Task.CompletedTask;
            }

            if (this.parameter.Identifier == "ACTUAL_TEMPERATURE")
            {
                var value = Convert.ToDouble(pvc.Value);

                context.Send(context.Parent, new ActualTemperatureChanged(value, this.id, pvc.Timestamp));
            }
        }

        return Task.CompletedTask;
    }
}

public record ActualTemperatureChanged(double Value, string Source, DateTimeOffset Timestamp);
