namespace Palantir;

public record Shutter : Device
{
    public decimal Level { get; init; }
}