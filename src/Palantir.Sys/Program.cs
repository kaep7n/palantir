using Google.Protobuf.WellKnownTypes;
using Palantir;
using Palantir.Apartment;
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

Proto.Log.SetLoggerFactory(
    LoggerFactory.Create(l => l
        .AddSerilog()
        .SetMinimumLevel(LogLevel.Information)
        )
    );

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration)
    => configuration.ReadFrom.Configuration(context.Configuration.GetSection("Logging"))
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(p =>
{
    var actorSystemConfig = ActorSystemConfig
        .Setup()
        .WithMetrics()
        .WithDeadLetterThrottleCount(3)
        .WithDeadLetterThrottleInterval(TimeSpan.FromSeconds(1))
        .WithDeveloperSupervisionLogging(true)
        .WithDeadLetterRequestLogging(true)
        .WithDeveloperThreadPoolStatsLogging(true);

    var kvStore = new InMemoryKeyValueStore();

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

builder.Services.AddSingleton(provider => provider.GetRequiredService<ActorSystem>().Cluster());

builder.Services.AddTransient<RootActor>();

builder.Services.AddHomatic(builder.Configuration.GetValue<string>("Homatic:Url")!);

builder.Services.AddHostedService<ActorSystemService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();