using System.Text.Json.Serialization;

namespace Palantir.Homatic.Mock;

public record SetValueRequest(
    [property: JsonPropertyName("v")] object Value
);
