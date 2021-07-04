using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Palantir.Homematic
{
    public record ParameterInformation
    {
        public string Control { get; init; }
        public int Default { get; init; }
        public int Flags { get; init; }
        public string Id { get; init; }
        public string Identifier { get; init; }
        public int Maximum { get; init; }
        public int Minimum { get; init; }
        public string MqttStatusTopic { get; init; }
        public int Operations { get; init; }
        public IEnumerable<object> Special { get; } = new List<object>();
        public int TabOrder { get; init; }
        public string Title { get; init; }
        public string Type { get; init; }
        public string Unit { get; init; }

        [JsonPropertyName("~links")]
        public IEnumerable<Link> Links { get; init; } = new List<Link>();
    }

}
