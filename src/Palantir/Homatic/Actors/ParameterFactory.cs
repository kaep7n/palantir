using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Net.Http;

namespace Palantir.Homatic.Actors
{
    public class ParameterFactory : IParameterFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ParameterFactory(IServiceProvider serviceProvider)
            => this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        public Props CreateProps(string identifer)
            => Props.FromProducer(() => new Parameter(
                identifer,
                this.serviceProvider.GetRequiredService<IHttpClientFactory>(),
                this.serviceProvider.GetRequiredService<ILogger<Parameter>>()
            ));
    }
}
