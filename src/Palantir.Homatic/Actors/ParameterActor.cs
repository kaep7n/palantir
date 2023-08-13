using Microsoft.Extensions.Logging;
using Proto;

namespace Palantir.Homatic.Actors;

public class ParameterActor(PID apiPool, string id, ILogger<ParameterActor> logger) : BaseActor(apiPool, id, logger)
{
    private Parameter? parameter;
    private DateTimeOffset? currentValueTimestamp;
    private object? currentValue;

    public override Task ReceiveAsync(IContext context)
    {
        base.ReceiveAsync(context);

        return context.Message switch
        {
            GetParameterResult msg => this.OnGetParameterResult(msg),
            GetParameterValueResult msg => this.OnGetParameterValueResult(msg),
            ParameterValueChanged msg => this.OnParameterValueChanged(context, msg),
            _ => Task.CompletedTask
        };
    }

    protected override Task OnStarted(IContext context)
    {
        context.Request(this.apiPool, new GetParameter(this.id), context.Self);
        context.Request(this.apiPool, new GetParameterValue(this.id), context.Self);

        return base.OnStarted(context);
    }

    private Task OnGetParameterResult(GetParameterResult msg)
    {
        this.parameter = msg.Parameter;

        return Task.CompletedTask;
    }

    private Task OnGetParameterValueResult(GetParameterValueResult msg)
    {
        this.currentValueTimestamp = msg.Timestamp;
        this.currentValue = msg.Value;

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

            this.currentValueTimestamp = pvc.Timestamp;
            this.currentValue = pvc.Value;

            if (context.Parent is null)
                return Task.CompletedTask;
            if (this.parameter is null)
                return Task.CompletedTask;

            if (this.parameter.Identifier == "ACTUAL_TEMPERATURE")
            {
                var value = Convert.ToDouble(pvc.Value);

                context.Send(
                    context.Parent,
                    new TemperatureChanged(
                        new Sender(context.Self.Address, context.Self.Id),
                        value,
                        pvc.Timestamp
                    )
                );
            }
            else if (this.parameter.Identifier == "SET_TEMPERATURE")
            {
                var value = Convert.ToDouble(pvc.Value);

                context.Send(
                    context.Parent,
                    new SetTemperatureChanged(
                        new Sender(context.Self.Address, context.Self.Id),
                        value,
                        pvc.Timestamp
                    )
                );
            }
        }

        return Task.CompletedTask;
    }
}