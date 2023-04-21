namespace Palantir;
public class RoomConfig
{
    public List<DeviceConfig> Devices { get; set; }
}

public class DeviceConfig
{
    public string Type { get; set; }

    public string Name { get; set; }
}