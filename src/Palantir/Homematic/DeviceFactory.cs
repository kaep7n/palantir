using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Net.Http;

namespace Palantir
{
    public class DeviceFactory : IDeviceFactory
    {
        private readonly IServiceProvider serviceProvider;

        public DeviceFactory(IServiceProvider serviceProvider)
            => this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        public Props CreateProps(string identifer)
            => Props.FromProducer(() => new Device(
                identifer,
                this.serviceProvider.GetRequiredService<IChannelFactory>(),
                this.serviceProvider.GetRequiredService<IHttpClientFactory>(),
                this.serviceProvider.GetRequiredService<ILogger<Device>>()
            ));
    }
}
