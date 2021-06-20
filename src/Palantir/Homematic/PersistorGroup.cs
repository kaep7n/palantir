using Palantir.Homatic;
using Proto;
using Proto.Router;
using System.Threading.Tasks;

namespace Palantir.Homematic
{
    public class PersistorGroup : IActor
    {
        public PersistorGroup()
        {
        }

        public Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
                var persistorProps = Props.FromProducer(() => new Persistor());
                var poolProps = context.NewRoundRobinPool(persistorProps, 5);
                var pid = context.Spawn(poolProps);

                context.System.EventStream.Subscribe<DeviceData>(msg =>
                {
                    context.Send(pid, msg);
                });
            }

            return Task.CompletedTask;
        }
    }
}
