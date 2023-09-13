using System.Text.Json.Serialization;

namespace Palantir.Homatic.Mock;

/// <summary>
/// VEAP Protocol Message
/// https://github.com/mdzio/veap/blob/master/README_de.md
/// </summary>
public record Veap(
    [property: JsonPropertyName("ts")] long Timestamp,
    [property: JsonPropertyName("v")] object Value,
    [property: JsonPropertyName("s")] int Status
);