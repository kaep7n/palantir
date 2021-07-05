
using System;

namespace Palantir
{
    public record DeviceData(string Device, string Channel, string Parameter, DateTimeOffset Timestamp, object Value, int Status);
}
