using Google.Protobuf;
using Proto;

namespace Palantir;

public partial class JoinRoom
{
    public JoinRoom(string deviceId, string channelId, string room)
    {
        this.DeviceId = deviceId;
        this.ChannelId = channelId;
        this.Room = room;
    }
}

public partial class RoomJoined
{
    public RoomJoined(PID pid)
    {
        this.PidBytes = pid.ToByteString();
    }

    public PID Pid => PID.Parser.ParseFrom(this.PidBytes);
}