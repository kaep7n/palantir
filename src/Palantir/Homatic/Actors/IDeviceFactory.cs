using Proto;

namespace Palantir.Homatic.Actors
{
    public interface IDeviceFactory
    {
        Props CreateProps(string identifer);
    }
}
