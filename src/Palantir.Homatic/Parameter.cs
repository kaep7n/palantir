﻿using System.Text.Json.Serialization;

namespace Palantir.Homatic;

public record Parameter(
    [property: JsonPropertyName("control")] string Control,
    [property: JsonPropertyName("default")] object Default,
    [property: JsonPropertyName("flags")] int Flags,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("identifier")] string Identifier,
    [property: JsonPropertyName("maximum")] object Maximum,
    [property: JsonPropertyName("minimum")] object Minimum,
    [property: JsonPropertyName("mqttStatusTopic")] string MqttStatusTopic,
    [property: JsonPropertyName("operations")] int Operations,
    [property: JsonPropertyName("tabOrder")] int TabOrder,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("unit")] string Unit,
    [property: JsonPropertyName("valueList")] IReadOnlyList<string> ValueList,
    [property: JsonPropertyName("~links")] IReadOnlyList<Link> Links
);
