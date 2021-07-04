using System.Collections.Generic;

namespace Palantir
{
    public record DeviceState(DeviceInformation Information, IEnumerable<ChannelData> Channels);
}
