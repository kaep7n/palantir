namespace Palantir;
public record Thermostat : BatteryDevice
{
    public decimal SetTemperature { get; init; }

    public decimal Temperature { get; init; }

    public decimal Humidity { get; init; }
}
