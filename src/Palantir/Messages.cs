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