using Palantir.Apartment;
using Palantir.Homatic.Extensions;
using Palantir.Sys;
using Proto;
using Proto.DependencyInjection;
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