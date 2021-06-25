using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Prometheus;
using Proto;
using Proto.DependencyInjection;
using Ubiquitous.Metrics.Prometheus;

namespace Palantir
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = LoggerFactory.Create(x => x.AddConsole().SetMinimumLevel(LogLevel.Information));
            Log.SetLoggerFactory(loggerFactory);

            var config = ActorSystemConfig.Setup().WithMetricsProviders(new PrometheusConfigurator());

            services.AddSingleton(serviceProvider => 
                    new ActorSystem(config)
                        .WithServiceProvider(serviceProvider)
                    );
            services.AddTransient<HomaticRoot>();
            services.AddTransient<PersistorGroup>();
            services.AddTransient<Persistor>();
            services.AddTransient<DeviceController>();
            services.AddTransient<Device>();

            services.AddHttpClient();
            services.AddHostedService(p => p.GetRequiredService<Worker>());
            services.AddSingleton<Worker>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Palantir", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
