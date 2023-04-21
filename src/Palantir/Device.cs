namespace Palantir;

public record Device
{
    public string? Name { get; set; }

    public DateTimeOffset LastModifiedAtUtc { get; init; }
}

public record BatteryDevice : Device
{
    public decimal BatteryPower { get; init; }
}