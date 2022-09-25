using System.Text.Json.Serialization;

namespace Palantir;

public record Devices(
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("identifier")] string Identifier,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("~links")] IReadOnlyList<Link> Links
);
