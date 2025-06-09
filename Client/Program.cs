using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Streams;
using Contracts;

namespace Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            // Adiciona o cliente Orleans
            builder.Services.AddOrleansClient(clientBuilder =>
            {
                clientBuilder
                    .UseLocalhostClustering()
                    .AddMemoryStreams("Default")
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

            // await host.StartAsync();

            // var client = host.Services.GetRequiredService<IClusterClient>();
            // Console.WriteLine("Client connected.");

            // var companyId = "company1";

            // var companyGrain = client.GetGrain<ICompanyGrain>(companyId);
            // Console.WriteLine("Clearing company data...");
            // //await companyGrain.ClearPerformanceAsync();

            // // Simular fator inicial
            // var streamProvider = client.GetStreamProvider("Default");
            // var factorStream = streamProvider.GetStream<FactorChangedEvent>(
            //     StreamId.Create("CompanyFactorUpdates", companyId)
            // );


            // // Definição de áreas e dados simulados
            // var areaUpdates = new[]
            // {
            //     new { AreaId = "areaA", Hours = 4.0M, Value = 160.0M },
            //     new { AreaId = "areaB", Hours = 6.0M, Value = 300.0M },
            //     new { AreaId = "areaC", Hours = 5.0M, Value = 200.0M },
            //     new { AreaId = "areaD", Hours = 10.0M, Value = 200.0M },
            // };

            // // Atualizar duas áreas e recalcular parcialmente
            // for (int i = 0; i < 2; i++)
            // {
            //     var update = areaUpdates[i];
            //     var areaGrain = client.GetGrain<IAreaGrain>($"company:{companyId}_area:{update.AreaId}");
            //     await areaGrain.UpdateOperationAsync(update.Hours, update.Value);

            //     Console.WriteLine($"Updated area {update.AreaId} with hours={update.Hours} and value={update.Value}.");
            // }

            // // Recalcular a empresa após atualizações parciais
            // Console.WriteLine("Recalculating company after partial updates...");
            // var avgPartial = await companyGrain.GetAveragePerformanceAsync();
            // Console.WriteLine($"[Partial] Company average performance: {avgPartial:N2}");

            // await Task.Delay(2000);

            // var initialFactor = 2.0m;
            // await factorStream.OnNextAsync(new FactorChangedEvent(companyId, initialFactor));
            // Console.WriteLine($"[Broadcast] Initial factor {initialFactor} sent.");

            // // Atualizar restante das áreas
            // for (int i = 2; i < areaUpdates.Length; i++)
            // {
            //     var update = areaUpdates[i];
            //     var areaGrain = client.GetGrain<IAreaGrain>($"company:{companyId}_area:{update.AreaId}");
            //     await areaGrain.UpdateOperationAsync(update.Hours, update.Value);

            //     Console.WriteLine($"Updated area {update.AreaId} with hours={update.Hours} and value={update.Value}.");
            // }

            // // Recalcular a empresa após todos os dados
            // Console.WriteLine("Recalculating company after full updates...");
            // var avgFinal = await companyGrain.GetAveragePerformanceAsync();
            // Console.WriteLine($"[Final] Company average performance: {avgFinal:N2}");

            // // Simular alteração de fator com broadcast
            // var newFactor = 4m;
            // await factorStream.OnNextAsync(new FactorChangedEvent(companyId, newFactor));
            // Console.WriteLine($"[Broadcast] New factor {newFactor} sent.");

            // await Task.Delay(2000);

            // var avgAfterFactor = await companyGrain.GetAveragePerformanceAsync();
            // Console.WriteLine($"[Recalculated] Company average performance after factor update: {avgAfterFactor:N2}");

            // Console.WriteLine("All operations complete. Press any key to exit.");
            // Console.ReadKey();

            // await host.StopAsync();
        }
    }
}
