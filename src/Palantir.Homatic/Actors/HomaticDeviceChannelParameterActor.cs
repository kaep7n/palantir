using Microsoft.Extensions.Logging;
using Palantir.Homatic.Http;
using Proto;

namespace Palantir.Homatic.Actors;

public class HomaticDeviceChannelParameterActor : IActor
{
    private readonly string deviceId;
    private readonly string channelId;
    private readonly string id;
    private readonly HomaticHttpClient homaticClient;
    private readonly ILogger<HomaticDeviceChannelParameterActor> logger;
    private Parameter parameter;
    private object currentValue;

    public HomaticDeviceChannelParameterActor(
        string deviceId,
        string channelId,
        string id,
        HomaticHttpClient homaticClient,
        ILogger<HomaticDeviceChannelParameterActor> logger)
    {
        this.deviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
        this.channelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
        this.id = id ?? throw new ArgumentNullException(nameof(id));
        this.homaticClient = homaticClient ?? throw new ArgumentNullException(nameof(homaticClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ReceiveAsync(IContext context)
    {
        if (context.Message is Started)
        {
            try
            {
                this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);

                this.parameter = await this.homaticClient.GetParameterAsync(this.deviceId, this.channelId, this.id);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, $"HomaticDeviceChannelParameterActor {this.deviceId}/{this.channelId}/{this.id}");
            }
        }
        if (context.Message is ParameterValueChanged pvc)
        {
            if (this.currentValue is null || !this.currentValue.Equals(pvc.Value))
            {
                this.logger.LogDebug(
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
        if (context.Message is Stopped)
        {
            this.logger.LogDebug("{type} ({pid}) has started", this.GetType(), context.Self);
        }
    }
}
