using Google.Protobuf;
using Proto;

namespace Palantir;

public partial class Join
{
    public Join(string deviceId, string channelId, string room)
    {
        this.DeviceId = deviceId;
        this.ChannelId = channelId;
        this.Room = room;
    }
}

public partial class Joined
{
    public Joined(PID pid)
    {
        this.PidBytes = pid.ToByteString();
    }

    public PID Pid => PID.Parser.ParseFrom(this.PidBytes);
}