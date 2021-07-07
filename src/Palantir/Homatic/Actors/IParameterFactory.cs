using Proto;

namespace Palantir.Homatic.Actors
{
    public interface IParameterFactory
    {
        Props CreateProps(string identifer);
    }
}
