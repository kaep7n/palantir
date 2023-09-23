using Palantir.Homatic.Mock.Json;
using System.Collections.Immutable;

namespace Palantir.Homatic.Mock;

public record Parameter
{
    private readonly JsonParameter raw;

    public Parameter(JsonParameter raw)
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