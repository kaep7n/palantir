using Microsoft.Extensions.DependencyInjection;
using Palantir.Homatic.Actors;
using Palantir.Homatic.Http;

namespace Palantir.Homatic.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHomatic(this IServiceCollection services, string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        services.AddTransient<RootActor>();
        services.AddTransient<MqttActor>();
        services.AddTransient<DeviceActor>();
        services.AddTransient<ChannelActor>();
        services.AddTransient<ParameterActor>();
        services.AddTransient<ApiActor>();

        services.AddHttpClient<HomaticHttpClient>((p, c) =>
        {
            c.BaseAddress = new Uri(url);
        });

        return services;
    }
}
