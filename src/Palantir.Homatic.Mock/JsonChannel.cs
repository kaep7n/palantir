using System.Text.Json;
using System.Text.Json.Serialization;

namespace Palantir.Homatic.Mock;

public record JsonChannel(
     [property: JsonPropertyName("address")] string Address,
     [property: JsonPropertyName("aesActive")] int AesActive,
     [property: JsonPropertyName("availableFirmware")] string AvailableFirmware,
     [property: JsonPropertyName("children")] JsonElement Children,
     [property: JsonPropertyName("direction")] int Direction,
     [property: JsonPropertyName("firmware")] string Firmware,
     [property: JsonPropertyName("flags")] int Flags,
     [property: JsonPropertyName("group")] string Group,
     [property: JsonPropertyName("identifier")] string Identifier,
     [property: JsonPropertyName("index")] int Index,
     [property: JsonPropertyName("interface")] string Interface,
     [property: JsonPropertyName("linkSourceRoles")] string LinkSourceRoles,
     [property: JsonPropertyName("linkTargetRoles")] string LinkTargetRoles,
     [property: JsonPropertyName("paramsets")] IReadOnlyList<string> Paramsets,
     [property: JsonPropertyName("parent")] string Parent,
     [property: JsonPropertyName("parentType")] string ParentType,
     [property: JsonPropertyName("rfAddress")] int RfAddress,
     [property: JsonPropertyName("roaming")] int Roaming,
     [property: JsonPropertyName("rxMode")] int RxMode,
     [property: JsonPropertyName("team")] string Team,
     [property: JsonPropertyName("teamChannels")] JsonElement TeamChannels,
     [property: JsonPropertyName("teamTag")] string TeamTag,
     [property: JsonPropertyName("title")] string Title,
     [property: JsonPropertyName("type")] string Type,
     [property: JsonPropertyName("version")] int Version,
     [property: JsonPropertyName("~links")] IReadOnlyList<JsonLink> Links
 );
