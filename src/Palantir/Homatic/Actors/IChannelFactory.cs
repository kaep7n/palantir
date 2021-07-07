using Proto;

namespace Palantir.Homatic.Actors
{
    public interface IChannelFactory
    {
        Props CreateProps(string identifer);
    }
}
