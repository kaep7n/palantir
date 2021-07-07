using System;

namespace Palantir.Homatic
{
    public record DeviceParameterValue(string Device, string Channel, string Parameter, DateTimeOffset Timestamp, object Value, int Status);
}
