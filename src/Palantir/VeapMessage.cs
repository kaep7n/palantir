using System.Text.Json.Serialization;

namespace Palantir
{
    /// <summary>
    /// VEAP Protocol Message
    /// https://github.com/mdzio/veap/blob/master/README_de.md
    /// </summary>
    public record VeapMessage(
        [property: JsonPropertyName("ts")] long Timestamp,
        [property: JsonPropertyName("v")] object Value,
        [property: JsonPropertyName("s")] int Status
    );
}
