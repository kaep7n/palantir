using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Palantir.Homatic
{
    public record ParameterInformation
    {
        public string Control { get; init; }

        public object Default { get; init; }

        public int Flags { get; init; }

        public string Id { get; init; }

        public string Identifier { get; init; }

        public object Maximum { get; init; }

        public object Minimum { get; init; }

        public string MqttStatusTopic { get; init; }

        public int Operations { get; init; }

        public int TabOrder { get; init; }

        public string Title { get; init; }

        public string Type { get; init; }

        public string Unit { get; init; }

        [JsonPropertyName("~links")]
        public IEnumerable<Link> Links { get; init; } = new List<Link>();
    }

}
