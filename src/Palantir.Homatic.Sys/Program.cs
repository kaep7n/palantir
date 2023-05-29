using Google.Protobuf.WellKnownTypes;
using Palantir;
using Palantir.Homatic.Extensions;
using Palantir.Sys;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Cache;
using Proto.Cluster.Consul;
using Proto.Cluster.Partition;
using Proto.Cluster.PubSub;
using Proto.DependencyInjection;
using Proto.Remote;
using Proto.Remote.GrpcNet;
using Serilog;
using StackExchange.Redis;

Proto.Log.SetLoggerFactory(
    LoggerFactory.Create(l => l
        .AddSerilog()
        .SetMinimumLevel(LogLevel.Information)
        )
    );

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(p =>
        {
            var actorSystemConfig = ActorSystemConfig
                .Setup()
                .WithMetrics()
                .WithDeadLetterThrottleCount(3)
                .WithDeadLetterThrottleInterval(TimeSpan.FromSeconds(1))
                .WithDeveloperSupervisionLogging(true)
                .WithDeadLetterRequestLogging(true)
                .WithDeveloperThreadPoolStatsLogging(true);

            var multiplexer = ConnectionMultiplexer.Connect("localhost:6379");
            var db = multiplexer.GetDatabase();
            var kvStore = new RedisKeyValueStore(db, 50);

            var remoteConfig = GrpcNetRemoteConfig
                    .BindToLocalhost()
                    .WithProtoMessages(EmptyReflection.Descriptor)
                    .WithProtoMessages(MessagesReflection.Descriptor)
                    .WithRemoteDiagnostics(true);

            var clusterName = "palantir";
            var clusterProvider = new ConsulProvider(new ConsulProviderConfig());

            var actorSystem = new ActorSystem(actorSystemConfig);

            actorSystem
                    .WithServiceProvider(p)
                    .WithRemote(remoteConfig)
                    .WithCluster(ClusterConfig
                        .Setup(clusterName, clusterProvider, new PartitionIdentityLookup())
                        // explicit topic actor registration is needed to provide a key value store implementation
                        .WithClusterKind(TopicActor.Kind, Props.FromProducer(() => new TopicActor(kvStore)))
                    )
                    .Cluster()
                    .WithPidCacheInvalidation();

            return actorSystem;
        });

        services.AddHostedService<ActorSystemService>();
        services.AddHomatic("http://192.168.2.101:2121/");
    })
    .UseSerilog((context, configuration)
        => configuration.ReadFrom.Configuration(context.Configuration.GetSection("Logging"))
    )
    .Build();

host.Run();
