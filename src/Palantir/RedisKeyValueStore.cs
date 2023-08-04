using Google.Protobuf;
using Proto.Cluster.PubSub;
using Proto.Utils;
using StackExchange.Redis;

namespace Palantir;

public class RedisKeyValueStore(IDatabase db, int maxConcurrency) : ConcurrentKeyValueStore<Subscribers>(new AsyncSemaphore(maxConcurrency))
{
    private readonly IDatabase db = db;

    protected override async Task<Subscribers> InnerGetStateAsync(string id, CancellationToken ct)
    {
        var value = await db.StringGetAsync(Key(id));
        if (value.IsNullOrEmpty)
            return new Subscribers();

        return Subscribers.Parser.ParseFrom(value);
    }

    protected override Task InnerSetStateAsync(string id, Subscribers state, CancellationToken ct)
        => db.StringSetAsync(Key(id), state.ToByteArray());

    protected override Task InnerClearStateAsync(string id, CancellationToken ct)
        => db.KeyDeleteAsync(Key(id));

    private string Key(string id) => $"subscribers:{id}";
}