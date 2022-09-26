using Palantir.Apartment;
using Palantir.Homatic.Extensions;
using Palantir.Sys;
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
     .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3} {SourceContext}] {Message}{NewLine}{Exception}")
     .WriteTo.File("logs/log.txt", outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3} {SourceContext}] {Message}{NewLine}{Exception}");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(p => new ActorSystem().WithServiceProvider(p));

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