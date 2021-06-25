using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Palantir
{
    public record DeviceQueryResult
    {
        public DeviceQueryResult(string identifier, string title, string description, IEnumerable<Link> links)
        {
            this.Identifier = identifier;
            this.Title = title;
            this.Description = description;
            this.Links = links;
        }

        public string Identifier { get; init; }

        public string Title { get; init; }

        public string Description { get; init; }

        [JsonPropertyName("~links")]
        public IEnumerable<Link> Links { get; init; }
    }
}
