using Proto;
using System.Threading.Tasks;

namespace Palantir
{
    public class Persistor : IActor
    {
        public Persistor()
        {
        }

        public Task ReceiveAsync(IContext context)
        {
            if(context.Message is Started)
            {
            }
            if (context.Message is DeviceData msg)
            {
            }

            return Task.CompletedTask;
        }
    }
}
