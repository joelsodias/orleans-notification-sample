using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Streams;
using Contracts;
using Orleans.Streaming.Redis;
using Common.Extensions;
using Common.Constants;

namespace Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var env = builder.Environment;

            var configuration = builder.Configuration
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                        .AddEnvironmentVariables()
                        .Build();
                

            // Adiciona o cliente Orleans
            builder.Services.AddOrleansClient(configuration, clientBuilder =>
            {
                var configuration = clientBuilder.Configuration;

                clientBuilder
                    .UseLocalhostClustering()

                    .AddRedisStreamClient(StreamsConstants.ProviderName, configuration.GetRedisConfigurationOptions())

                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "dev";
                        options.ServiceId = "CompanyPerformanceApp";
                    });
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.MapControllers();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            await app.RunAsync();

        }
    }
}
