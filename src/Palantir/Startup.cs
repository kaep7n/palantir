using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Palantir.Homatic;
using Palantir.Homatic.Actors;
using Prometheus;
using Proto;
using Proto.DependencyInjection;
using Ubiquitous.Metrics.Prometheus;

namespace Palantir
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
            => this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ElasticConfiguration>(this.Configuration.GetSection("elasticSearch"));
            services.AddTransient<PersistorClient>();

            var config = ActorSystemConfig.Setup().WithMetricsProviders(new PrometheusConfigurator());

            services.AddSingleton(serviceProvider =>
                    new ActorSystem(config)
                        .WithServiceProvider(serviceProvider)
                    );
            services.AddTransient<Root>();
            services.AddTransient<PersistorGroup>();
            services.AddTransient<Persistor>();
            services.AddTransient<DeviceController>();
            services.AddTransient<Device>();

            services.AddTransient<IDeviceFactory, DeviceFactory>();
            services.AddTransient<IChannelFactory, ChannelFactory>();
            services.AddTransient<IParameterFactory, ParameterFactory>();

            services.AddHttpClient();
            services.AddHostedService(p => p.GetRequiredService<ConnectorService>());
            services.AddSingleton<ConnectorService>();
            services.AddHostedService<PersistorService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Palantir", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Palantir v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }
    }
}
