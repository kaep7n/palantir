using Proto;

namespace Palantir
{
    public interface IParameterFactory
    {
        Props CreateProps(string identifer);
    }
}