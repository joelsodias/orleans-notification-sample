using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Threading.Tasks;

namespace SiloHost
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseOrleans(siloBuilder =>
                {
                    siloBuilder
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "dev";
                            options.ServiceId = "CompanyPerformanceApp";
                        })
                        .UseLocalhostClustering()
                        .AddMemoryGrainStorage("PubSubStore")
                        .AddMemoryStreams("Default")
                        .ConfigureLogging(logging => logging.AddConsole());
                })
                .Build();

            await host.StartAsync();

            // Mantém o silo ativo
            await host.WaitForShutdownAsync();
        }
    }
}
