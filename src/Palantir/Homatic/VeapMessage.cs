using System.Text.Json.Serialization;

namespace Palantir.Homatic
{
    /// <summary>
    /// VEAP Protocol Message
    /// https://github.com/mdzio/veap/blob/master/README_de.md
    /// </summary>
    public record VeapMessage
    {
        public VeapMessage(long timestamp, object value, int status)
        {
            this.Timestamp = timestamp;
            this.Value = value;
            this.Status = status;
        }

        [JsonPropertyName("ts")]
        public long Timestamp { get; set; }

        [JsonPropertyName("v")]
        public object Value { get; set; }

        [JsonPropertyName("s")]
        public int Status { get; set; }
    }
}
