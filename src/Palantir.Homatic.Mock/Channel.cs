using Palantir.Homatic.Mock.Json;
using System.Collections.Immutable;
using System.Text.Json;

namespace Palantir.Homatic.Mock;

public record Channel
{
    private readonly JsonChannel raw;

    public Channel(JsonChannel raw, IImmutableList<string> rooms, IImmutableList<Parameter> parameters)
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
        this.Rooms = rooms;
        this.Parameters = parameters;
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

    public IImmutableList<Parameter> Parameters { get; init; }

    public JsonChannel GetRaw() => this.raw;
}
