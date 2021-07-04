using Proto;

namespace Palantir
{
    public interface IChannelFactory
    {
        Props CreateProps(string identifer);
    }
}