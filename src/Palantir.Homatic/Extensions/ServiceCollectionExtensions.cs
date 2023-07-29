using Microsoft.Extensions.DependencyInjection;
using Palantir.Homatic.Actors;
using Palantir.Homatic.Http;

namespace Palantir.Homatic.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHomatic(this IServiceCollection services, string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        services.AddTransient<HomaticActor>();
        services.AddTransient<HomaticMqttActor>();
        services.AddTransient<HomaticDeviceActor>();
        services.AddTransient<HomaticDeviceChannelActor>();
        services.AddTransient<HomaticDeviceChannelParameterActor>();
        services.AddTransient<HttpReceiverArctor>();

        services.AddHttpClient<HomaticHttpClient>((p, c) =>
        {
            c.BaseAddress = new Uri(url);
        });

        return services;
    }
}
