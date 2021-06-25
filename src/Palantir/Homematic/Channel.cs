using System.Collections.Generic;

namespace Palantir
{
    public record Channel(int Number, Dictionary<string, Data> Params);
}
