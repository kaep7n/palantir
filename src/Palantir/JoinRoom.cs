using Proto;

namespace Palantir;

public record JoinRoom(string DeviceId, string ChannelId, string Room);

public record RoomJoined(PID Room);
