namespace Palantir;

public record Lock : BatteryDevice
{
    public bool Open { get; init; }
}