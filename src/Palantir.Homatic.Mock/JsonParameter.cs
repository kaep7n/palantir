using System.Text.Json;
using System.Text.Json.Serialization;

namespace Palantir.Homatic.Mock;

public record JsonParameter(
    [property: JsonPropertyName("control")] string Control,
    [property: JsonPropertyName("default")] JsonElement Default,
    [property: JsonPropertyName("flags")] int Flags,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("identifier")] string Identifier,
    [property: JsonPropertyName("maximum")] JsonElement Maximum,
    [property: JsonPropertyName("minimum")] JsonElement Minimum,
    [property: JsonPropertyName("mqttStatusTopic")] string MqttStatusTopic,
    [property: JsonPropertyName("operations")] int Operations,
    [property: JsonPropertyName("tabOrder")] int TabOrder,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("unit")] string Unit,
    [property: JsonPropertyName("valueList")] IReadOnlyList<string> ValueList,
    [property: JsonPropertyName("~links")] IReadOnlyList<JsonLink> Links
)
{
    public object? GetDefaultValue() => this.Type switch
    {
        "ACTION" => this.Default.GetBoolean(),
        "BOOL" => this.Default.GetBoolean(),
        "ENUM" => this.Default.GetInt32(),
        "FLOAT" => this.Default.GetDouble(),
        "INTEGER" => this.Default.GetInt32(),
        "STRING" => this.Default.GetString(),
        _ => null
    };

    public object? GetMinimumValue() => this.Type switch
    {
        "ACTION" => this.Minimum.GetBoolean(),
        "BOOL" => this.Minimum.GetBoolean(),
        "ENUM" => this.Minimum.GetInt32(),
        "FLOAT" => this.Minimum.GetDouble(),
        "INTEGER" => this.Minimum.GetInt32(),
        "STRING" => this.Minimum.GetString(),
        _ => null
    };

    public object? GetMaximumValue() => this.Type switch
    {
        "ACTION" => this.Maximum.GetBoolean(),
        "BOOL" => this.Maximum.GetBoolean(),
        "ENUM" => this.Maximum.GetInt32(),
        "FLOAT" => this.Maximum.GetDouble(),
        "INTEGER" => this.Maximum.GetInt32(),
        "STRING" => this.Maximum.GetString(),
        _ => null
    };
}
