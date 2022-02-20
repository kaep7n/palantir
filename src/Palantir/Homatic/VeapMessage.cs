using System.Text.Json.Serialization;

namespace Palantir.Homatic
{
    /// <summary>
    /// VEAP Protocol Message
    /// https://github.com/mdzio/veap/blob/master/README_de.md
    /// </summary>
    public record VeapMessage(long Timestamp, object Value, int Status)
    {
        [JsonPropertyName("ts")]
        public long Timestamp { get; set; } = Timestamp;

        [JsonPropertyName("v")]
        public object Value { get; set; } = Value;

        [JsonPropertyName("s")]
        public int Status { get; set; } = Status;
    }
}
