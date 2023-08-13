using Proto;

namespace Palantir;

public partial class JoinRoom
{
    public JoinRoom(string id, PID sender)
    {
        this.Id = id;
        this.Sender = new Sender { Address = sender.Address, Id = sender.Id };
    }
}

public partial class RoomJoined
{

}

public partial class Sender
{
    public Sender(string address, string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(address, nameof(address));
        ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

        this.Address = address;
        this.Id = id;
    }
}

public partial class TemperatureChanged
{
    public TemperatureChanged(Sender sender, double value, DateTimeOffset timestamp)
    {
        this.Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        this.Value = value;
        this.Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(timestamp);
    }
}

public partial class SetTemperatureChanged
{
    public SetTemperatureChanged(Sender sender, double value, DateTimeOffset timestamp)
    {
        this.Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        this.Value = value;
        this.Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(timestamp);
    }
}