using Proto;
using Proto.DependencyInjection;

namespace Palantir
{
    public class HomaticActor : IActor
    {
        private readonly HomaticHttpClient homaticClient;
        private readonly IEnumerable<Room> rooms;
        private readonly ILogger<HomaticActor> logger;

        private Dictionary<string, PID> devices = new Dictionary<string, PID>();
        private PID mqtt;

        public HomaticActor(
            HomaticHttpClient homaticClient,
            IEnumerable<Room> rooms,
            ILogger<HomaticActor> logger)
        {
            this.homaticClient = homaticClient ?? throw new ArgumentNullException(nameof(homaticClient));
            this.rooms = rooms ?? throw new ArgumentNullException(nameof(rooms));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
                try
                {
                    logger.LogDebug("{type} ({pid}) has started", GetType(), context.Self);

                    var devices = await homaticClient.GetDevicesAsync();

                    foreach (var link in devices.Links)
                    {
                        if (link.Href == "..")
                            continue;

                        var props = context.System.DI().PropsFor<HomaticDeviceActor>(link.Href);
                        var pid = context.Spawn(props);

                        this.devices.Add(link.Href, pid);
                    }

                    var mqttProps = context.System.DI().PropsFor<HomaticMqttActor>();
                    mqtt = context.Spawn(mqttProps);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "HomaticActor");
                }
            }
            if (context.Message is ParameterValueChanged pvc)
            {
                var device = this.devices[pvc.Device];
                context.Forward(device);
            }
            if (context.Message is JoinRoom joinRoom)
            {
                var roomPid = this.GetRoomPid(joinRoom.Room);
                context.Forward(roomPid);
            }
            if (context.Message is Stopped)
            {
                logger.LogDebug("{type} ({pid}) has started", GetType(), context.Self);
            }
        }

        private PID GetRoomPid(string homaticRoomId)
            => homaticRoomId switch
            {
                "1230" => this.rooms.First(r => r.Name == "Esszimmer").Id,
                "1226" => this.rooms.First(r => r.Name == "Küche").Id,
                "1228" => this.rooms.First(r => r.Name == "Leon").Id,
                "1229" => this.rooms.First(r => r.Name == "Linus").Id,
                "1227" => this.rooms.First(r => r.Name == "Schlafzimmer").Id,
                "1225" => this.rooms.First(r => r.Name == "Wohnzimmer").Id,
                "1231" => this.rooms.First(r => r.Name == "Bad").Id,
                _ => throw new InvalidOperationException($"Room mapping for id {homaticRoomId} not found."),
            };
    }
}
