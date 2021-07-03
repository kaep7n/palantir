using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;

namespace Palantir
{
    public class ElasticConfiguration
    {
        public IEnumerable<Uri> Nodes { get; set; }
    }

    public class PersistorClient
    {
        private readonly ElasticConfiguration options;

        public PersistorClient(IOptionsSnapshot<ElasticConfiguration> options)
        {
            if (options?.Value is null) throw new ArgumentNullException(nameof(options));

            this.options = options.Value;

            var pool = new StaticConnectionPool(this.options.Nodes);
            var settings = new ConnectionSettings(pool);
            this.Client = new ElasticClient(settings);
        }

        public ElasticClient Client { get; }
    }
}
