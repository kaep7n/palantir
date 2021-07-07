using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Net.Http;

namespace Palantir.Homatic.Actors
{
    public class ChannelFactory : IChannelFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ChannelFactory(IServiceProvider serviceProvider)
            => this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        public Props CreateProps(string identifer)
            => Props.FromProducer(() => new Channel(
                identifer,
                this.serviceProvider.GetRequiredService<IParameterFactory>(),
                this.serviceProvider.GetRequiredService<IHttpClientFactory>(),
                this.serviceProvider.GetRequiredService<ILogger<Channel>>()
            ));
    }
}
