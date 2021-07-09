using Proto;

namespace Palantir.Homatic.Actors
{
    public interface IParameterFactory
    {
        Props CreateProps(string identifer, DeviceInformation parentDevice, ChannelInformation parentChannel);
    }
}
