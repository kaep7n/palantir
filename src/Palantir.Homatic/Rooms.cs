using System.Collections.ObjectModel;

namespace Palantir.Homatic;

public static class Rooms
{
    private static readonly ReadOnlyDictionary<string, string> rooms = new Dictionary<string, string>()
    {
        { "1230", "Esszimmer" },
        { "1226", "Küche" },
        { "1228", "Leon" },
        { "1229", "Linus" },
        { "1227", "Schlafzimmer" },
        { "1225", "Wohnzimmer" },
        { "1231", "Bad" }
    }.AsReadOnly();

    public static string GetClusterIdentity(string homaticId)
    {
        return rooms[homaticId];
    }
}
