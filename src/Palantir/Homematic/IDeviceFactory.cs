using Proto;

namespace Palantir
{
    public interface IDeviceFactory
    {
        Props CreateProps(string identifer);
    }
}