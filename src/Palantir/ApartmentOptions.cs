namespace Palantir;

public class ApartmentOptions
{
    public IEnumerable<RoomOptions> Rooms { get; set; } = Enumerable.Empty<RoomOptions>();
}
