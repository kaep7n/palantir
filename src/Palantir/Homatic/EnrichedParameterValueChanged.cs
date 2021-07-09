using System;

namespace Palantir.Homatic
{
    public record DeviceDescription(string Id, string Name);

    public record ChannelDescription(string Id, string Name);

    public record ParameterDescription(string Id, string Name);

    public record EnrichedParameterValueChanged(
        string Fingerprint,
        DateTimeOffset Timestamp,
        DeviceDescription Device,
        ChannelDescription Channel,
        ParameterDescription Parameter,
        object Value,
        int Status
    );
}
