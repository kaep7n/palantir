namespace Palantir.Homatic;

public record ParameterValueChanged(string Device, string Channel, string Parameter, DateTimeOffset Timestamp, object Value, int Status);

