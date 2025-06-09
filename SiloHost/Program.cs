using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Orleans;
using Orleans.Hosting;
using Orleans.Streams;
using Orleans.Streaming.Redis;
using Orleans.Configuration;

using Common.Extensions;
using Common.Constants;

namespace SiloHost
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .UseOrleans((context, siloBuilder) =>
                {
                    var configuration = context.Configuration;

                    siloBuilder
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "dev";
                            options.ServiceId = "CompanyPerformanceApp";
                        })
                        .UseLocalhostClustering()

                        .AddRedisStreamServer(StreamsConstants.ProviderName, configuration.GetRedisConfigurationOptions())

                        .AddRedisGrainStorage(
                            name: "redisStateStore",
                            configureOptions =>
                            {
                                configureOptions.ConfigurationOptions = configuration.GetRedisConfigurationOptions();
                            })
                        .AddRedisGrainStorageAsDefault(configureOptions =>
                            {
                                configureOptions.ConfigurationOptions = configuration.GetRedisConfigurationOptions();
                            })

                            ;
                })
                .Build();

            await host.StartAsync();
            await host.WaitForShutdownAsync();
        }
    }
}
