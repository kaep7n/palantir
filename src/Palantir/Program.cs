using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Palantir
{
    public class Program
    {
        public static void Main(string[] args)
            => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, configuration) =>
                {
                    configuration.Enrich.FromLogContext()
                        .WriteTo.Console()
                        .WriteTo.Elasticsearch(
                            new ElasticsearchSinkOptions(context.Configuration.GetValue<IEnumerable<Uri>>("elasticSearch:nodes"))
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
