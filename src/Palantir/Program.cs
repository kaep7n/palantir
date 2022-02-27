using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Text;

namespace Palantir
{
    public class Program
    {
        public static int Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, configuration) =>
                {
                    configuration.Enrich.FromLogContext()
                        .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
                        .WriteTo.Elasticsearch(
                            new ElasticsearchSinkOptions(new []{ new Uri("https://elastic.kaeptn.dev")})
                            {
                                IndexFormat = new StringBuilder()
                                    .Append(typeof(Program).Assembly.GetName().Name.ToLower())
                                    .Append('-')
                                    .Append("logs")
                                    .Append('-')
                                    .Append(context.HostingEnvironment.EnvironmentName?.ToLower())
                                    .Append('-')
                                    .Append(DateTime.UtcNow.ToString("yyyy-MM"))
                                    .ToString(),
                                AutoRegisterTemplate = true,
                                NumberOfShards = 2
                            })
                        .Enrich.WithMachineName()
                        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                        .ReadFrom.Configuration(context.Configuration);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
