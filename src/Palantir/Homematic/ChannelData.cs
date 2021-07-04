using System.Collections.Generic;

namespace Palantir
{
    public record ChannelData(int Number, Dictionary<string, Data> Params);
}
