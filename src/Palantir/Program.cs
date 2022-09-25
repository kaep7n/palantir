using Microsoft.Extensions.Options;
using Palantir;
using Proto;
using Proto.DependencyInjection;
using Serilog;
using Log = Proto.Log;

//Configure ProtoActor to use Console logger
Log.SetLoggerFactory(
    LoggerFactory.Create(l => l
        .AddConsole()
        .SetMinimumLevel(LogLevel.Information))
    );

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, l) =>
{
    l.MinimumLevel.Information()
     .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
     .WriteTo.Console(outputTemplate: "Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}")
     .WriteTo.File("logs/log.txt");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(p => new ActorSystem().WithServiceProvider(p));

builder.Services.AddTransient<ApartmentActor>();
builder.Services.AddTransient<HomaticActor>();
builder.Services.AddTransient<HomaticMqttActor>();

builder.Services.Configure<HomaticOptions>(builder.Configuration.GetSection("Homatic"));
builder.Services.AddHttpClient<HomaticHttpClient>((p, c) =>
{
    var homaticOptions = p.GetRequiredService<IOptions<HomaticOptions>>();
    c.BaseAddress = new Uri(homaticOptions.Value.Url);
});

builder.Services.AddHostedService<ActorSystemService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();