using System.Collections.Immutable;
using System.Text.Json;

namespace Palantir.Homatic.Mock;

public record FakeHomatic
{
    private readonly JsonDevices raw;

    public FakeHomatic(JsonDevices raw, List<FakeDevice> devices)
    {
        this.raw = raw;
        this.Devices = devices.ToImmutableList();
    }

    public IImmutableList<FakeDevice> Devices { get; init; }

    public JsonDevices GetRaw() => this.raw;

    public FakeDevice GetDevice(string deviceId)
        => this.Devices.First(d => d.Identifier == deviceId);

    public FakeChannel GetChannel(string deviceId, string channelId)
        => this.Devices.First(d => d.Identifier == deviceId)
            .Channels.First(c => c.Identifier == channelId);

    public FakeParameter GetParameter(string deviceId, string channelId, string parameterId)
        => this.Devices.First(d => d.Identifier == deviceId)
            .Channels.First(c => c.Identifier == channelId)
            .Parameters.First(p => p.Identifier == parameterId);

    public Veap GetParameterValue(string deviceId, string channelId, string parameterId)
    {
        var parameter = this.GetParameter(deviceId, channelId, parameterId);

        return new Veap(parameter.CurrentValueChanged.ToUnixTimeMilliseconds(), parameter.CurrentValue, 0);
    }
}

public record FakeDevice
{
    private JsonDevice raw;

    public FakeDevice(JsonDevice raw, List<FakeChannel> channels)
    {
        this.raw = raw;

        this.Address = raw.Address;
        this.AesActive = raw.AesActive;
        this.AvailableFirmware = raw.AvailableFirmware;
        this.Children = raw.Children.ToImmutableList();
        this.Direction = raw.Direction;
        this.Firmware = raw.Firmware;
        this.Flags = raw.Flags;
        this.Group = raw.Group;
        this.Identifier = raw.Identifier;
        this.Index = raw.Index;
        this.Interface = raw.Interface;
        this.InterfaceType = raw.InterfaceType;
        this.LinkSourceRoles = raw.LinkSourceRoles;
        this.LinkTargetRoles = raw.LinkTargetRoles;
        this.Paramsets = raw.Paramsets.ToImmutableList();
        this.Parent = raw.Parent;
        this.ParentType = raw.ParentType;
        this.RfAddress = raw.RfAddress;
        this.Roaming = raw.Roaming;
        this.RxMode = raw.RxMode;
        this.Team = raw.Team;
        this.TeamChannels = raw.TeamChannels;
        this.TeamTag = raw.TeamTag;
        this.Title = raw.Title;
        this.Type = raw.Type;
        this.Version = raw.Version;
        this.Channels = channels.ToImmutableList();
    }

    public string Address { get; init; }

    public int AesActive { get; init; }

    public string AvailableFirmware { get; init; }

    public IImmutableList<string> Children { get; init; }

    public int Direction { get; init; }

    public string Firmware { get; init; }

    public int Flags { get; init; }

    public string Group { get; init; }

    public string Identifier { get; init; }

    public int Index { get; init; }

    public string Interface { get; init; }

    public string InterfaceType { get; init; }

    public string LinkSourceRoles { get; init; }

    public string LinkTargetRoles { get; init; }

    public IImmutableList<string> Paramsets { get; init; }

    public string Parent { get; init; }

    public string ParentType { get; init; }

    public int RfAddress { get; init; }

    public int Roaming { get; init; }

    public int RxMode { get; init; }

    public string Team { get; init; }

    public JsonElement TeamChannels { get; init; }

    public string TeamTag { get; init; }

    public string Title { get; init; }

    public string Type { get; init; }

    public int Version { get; init; }

    public IImmutableList<FakeChannel> Channels { get; init; }

    public JsonDevice GetRaw() => this.raw;
}

public record FakeChannel
{
    private readonly JsonChannel raw;

    public FakeChannel(JsonChannel raw, List<string> rooms, List<FakeParameter> parameters)
    {
        this.raw = raw;

        this.Address = raw.Address;
        this.AesActive = raw.AesActive;
        this.AvailableFirmware = raw.AvailableFirmware;
        this.Children = raw.Children;
        this.Direction = raw.Direction;
        this.Firmware = raw.Firmware;
        this.Flags = raw.Flags;
        this.Group = raw.Group;
        this.Identifier = raw.Identifier;
        this.Index = raw.Index;
        this.Interface = raw.Interface;
        this.LinkSourceRoles = raw.LinkSourceRoles;
        this.LinkTargetRoles = raw.LinkTargetRoles;
        this.Paramsets = raw.Paramsets?.ToImmutableList() ?? ImmutableList<string>.Empty;
        this.Parent = raw.Parent;
        this.ParentType = raw.ParentType;
        this.RfAddress = raw.RfAddress;
        this.Roaming = raw.Roaming;
        this.RxMode = raw.RxMode;
        this.Team = raw.Team;
        this.TeamChannels = raw.TeamChannels;
        this.TeamTag = raw.TeamTag;
        this.Title = raw.Title;
        this.Type = raw.Type;
        this.Version = raw.Version;
        this.Rooms = rooms.ToImmutableList();
        this.Parameters = parameters.ToImmutableList();
    }

    public string Address { get; init; }

    public int AesActive { get; init; }

    public string AvailableFirmware { get; init; }

    public JsonElement Children { get; init; }

    public int Direction { get; init; }

    public string Firmware { get; init; }

    public int Flags { get; init; }

    public string Group { get; init; }

    public string Identifier { get; init; }

    public int Index { get; init; }

    public string Interface { get; init; }

    public string LinkSourceRoles { get; init; }

    public string LinkTargetRoles { get; init; }

    public IImmutableList<string> Paramsets { get; init; }

    public string Parent { get; init; }

    public string ParentType { get; init; }

    public int RfAddress { get; init; }

    public int Roaming { get; init; }

    public int RxMode { get; init; }

    public string Team { get; init; }

    public JsonElement TeamChannels { get; init; }

    public string TeamTag { get; init; }

    public string Title { get; init; }

    public string Type { get; init; }

    public int Version { get; init; }

    public IImmutableList<string> Rooms { get; init; }

    public IImmutableList<FakeParameter> Parameters { get; init; }

    public JsonChannel GetRaw() => this.raw;
}

public record FakeParameter
{
    private readonly JsonParameter raw;

    public FakeParameter(JsonParameter raw)
    {
        this.raw = raw;

        this.Control = raw?.Control;
        this.Flags = raw?.Flags;
        this.Id = raw?.Id;
        this.Identifier = raw?.Identifier;
        this.MqttStatusTopic = raw?.MqttStatusTopic;
        this.Operations = raw?.Operations;
        this.TabOrder = raw?.TabOrder;
        this.Title = raw?.Title;
        this.Type = raw?.Type;
        this.Unit = raw?.Unit;
        this.ValueList = raw?.ValueList?.ToImmutableList() ?? ImmutableList<string>.Empty;
        this.Default = raw.GetDefaultValue();
        this.Minimum = raw.GetMinimumValue();
        this.Maximum = raw.GetMaximumValue();
    }

    public string Control { get; init; }

    public int? Flags { get; init; }

    public string Id { get; init; }

    public string Identifier { get; init; }

    public string MqttStatusTopic { get; init; }

    public int? Operations { get; init; }

    public int? TabOrder { get; init; }

    public string Title { get; init; }

    public string Type { get; init; }

    public string Unit { get; init; }

    public IImmutableList<string> ValueList { get; init; }

    public object? Default { get; init; }

    public object? Minimum { get; init; }

    public object? Maximum { get; init; }

    public DateTimeOffset CurrentValueChanged { get; init; }

    public object? CurrentValue { get; init; }

    public JsonParameter GetRaw() => this.raw;
}