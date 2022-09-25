using System.Text.Json.Serialization;

namespace Palantir;

public class HomaticHttpClient
{
    private readonly HttpClient http;

    public HomaticHttpClient(HttpClient http)
        => this.http = http ?? throw new ArgumentNullException(nameof(http));

    public async Task<Devices> GetDevicesAsync()
        => await http.GetFromJsonAsync<Devices>("device")
            .ConfigureAwait(false);

    public async Task<Device> GetDeviceAsync(string id)
        => await http.GetFromJsonAsync<Device>($"device/{id}")
            .ConfigureAwait(false);

    public async Task<Channel> GetChannelAsync(string deviceId, string id)
        => await http.GetFromJsonAsync<Channel>($"device/{deviceId}/{id}")
            .ConfigureAwait(false);

    public async Task<Parameter> GetParameterAsync(string deviceId, string channelId, string parameterId)
        => await http.GetFromJsonAsync<Parameter>($"device/{deviceId}/{channelId}/{parameterId}")
                .ConfigureAwait(false);
}

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

public record Channel(
     [property: JsonPropertyName("address")] string Address,
     [property: JsonPropertyName("aesActive")] int AesActive,
     [property: JsonPropertyName("availableFirmware")] string AvailableFirmware,
     [property: JsonPropertyName("children")] object Children,
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
     [property: JsonPropertyName("teamChannels")] object TeamChannels,
     [property: JsonPropertyName("teamTag")] string TeamTag,
     [property: JsonPropertyName("title")] string Title,
     [property: JsonPropertyName("type")] string Type,
     [property: JsonPropertyName("version")] int Version,
     [property: JsonPropertyName("~links")] IReadOnlyList<Link> Links
 );
