using System.Text.Json.Serialization;

namespace Palantir.Homatic.Mock;

public record Link(
    [property: JsonPropertyName("rel")] string Rel,
    [property: JsonPropertyName("href")] string Href,
    [property: JsonPropertyName("title")] string Title
);
