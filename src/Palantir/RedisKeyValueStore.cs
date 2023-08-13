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
        var value = await this.db.StringGetAsync(this.Key(id));
        if (value.IsNullOrEmpty)
            return new Subscribers();

        return Subscribers.Parser.ParseFrom(value);
    }

    protected override Task InnerSetStateAsync(string id, Subscribers state, CancellationToken ct)
        => this.db.StringSetAsync(this.Key(id), state.ToByteArray());

    protected override Task InnerClearStateAsync(string id, CancellationToken ct)
        => this.db.KeyDeleteAsync(this.Key(id));

    private string Key(string id) => $"subscribers:{id}";
}