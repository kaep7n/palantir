using Google.Protobuf.WellKnownTypes;
using Palantir;
using Palantir.Api;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Cache;
using Proto.Cluster.Partition;
using Proto.Cluster.PubSub;
using Proto.Cluster.Testing;
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

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
    var clusterProvider = new TestProvider(new TestProviderOptions(), new InMemAgent());

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

builder.Services.AddHostedService<ActorSystemService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
