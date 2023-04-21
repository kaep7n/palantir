namespace Palantir;

public record Dimmer : Device
{
    public decimal Level { get; init; }
}