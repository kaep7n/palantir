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

}