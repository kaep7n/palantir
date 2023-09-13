using System.Text.Json.Serialization;

namespace Palantir.Homatic.Mock;

public record JsonDevices(
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("identifier")] string Identifier,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("~links")] IReadOnlyList<JsonLink> Links
);
