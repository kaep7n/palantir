using System.Text.Json.Serialization;

namespace Palantir.Homatic.Mock.Json;

public record JsonLink(
    [property: JsonPropertyName("rel")] string Rel,
    [property: JsonPropertyName("href")] string Href,
    [property: JsonPropertyName("title")] string Title
);
