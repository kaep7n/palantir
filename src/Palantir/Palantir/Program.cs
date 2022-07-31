using Microsoft.Extensions.Options;
using Palantir;
using Proto;
using Proto.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

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