using System.Text.Json;
using System.Text.Json.Serialization;

namespace Palantir.Homatic.Mock.Json;

public record JsonParameter(
    [property: JsonPropertyName("control")] string Control,
    [property: JsonPropertyName("default")] JsonElement? Default,
    [property: JsonPropertyName("flags")] int Flags,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("identifier")] string Identifier,
    [property: JsonPropertyName("maximum")] JsonElement? Maximum,
    [property: JsonPropertyName("minimum")] JsonElement? Minimum,
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
    public object? GetDefaultValue()
        => this.Default is not null
            ? this.Type switch
            {
                "ACTION" => this.Default.Value.GetBoolean(),
                "BOOL" => this.Default.Value.GetBoolean(),
                "ENUM" => this.Default.Value.GetInt32(),
                "FLOAT" => this.Default.Value.GetDouble(),
                "INTEGER" => this.Default.Value.GetInt32(),
                "STRING" => this.Default.Value.GetString(),
                _ => null
            }
            : null;

    public object? GetMinimumValue() =>
        this.Minimum is not null
            ? this.Type switch
            {
                "ACTION" => this.Minimum.Value.GetBoolean(),
                "BOOL" => this.Minimum.Value.GetBoolean(),
                "ENUM" => this.Minimum.Value.GetInt32(),
                "FLOAT" => this.Minimum.Value.GetDouble(),
                "INTEGER" => this.Minimum.Value.GetInt32(),
                "STRING" => this.Minimum.Value.GetString(),
                _ => null
            }
            : null;

    public object? GetMaximumValue() =>
        this.Maximum is not null
        ? this.Type switch
        {
            "ACTION" => this.Maximum.Value.GetBoolean(),
            "BOOL" => this.Maximum.Value.GetBoolean(),
            "ENUM" => this.Maximum.Value.GetInt32(),
            "FLOAT" => this.Maximum.Value.GetDouble(),
            "INTEGER" => this.Maximum.Value.GetInt32(),
            "STRING" => this.Maximum.Value.GetString(),
            _ => null
        }
        : null;
}
