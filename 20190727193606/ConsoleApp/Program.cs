using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var argsHelper = ArgHelper.Load(args);

            if (argsHelper.HasHelp)
            {
                return;
            }

            var host = new HostBuilder()
                .UseEnvironment(argsHelper.EnvironmentName)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder
                        .AddJsonFile("appsettings.json");

                    var environmentFileName = $"appsettings.{context.HostingEnvironment.EnvironmentName}.json";

                    if (File.Exists(Path.Combine(Environment.CurrentDirectory, environmentFileName)))
                    {
                        builder.AddJsonFile(environmentFileName);
                    }
                })
                .ConfigureLogging((context, builder) =>
                {
                    var logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(context.Configuration)
                        //.WriteTo.Console(LogEventLevel.Debug, "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                        .CreateLogger();

                    builder.AddSerilog(logger);
                })
                .ConfigureLogging(builder =>
                {
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<DataCreator>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
