using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Streams;
using Orleans.Persistence.Redis;
using StackExchange.Redis;

namespace Orleans.Streaming.Redis;

public static class ConfigurationExt
{
    public static ISiloBuilder AddRedisStream(this ISiloBuilder siloBuilder, string providerName, ConfigurationOptions redisConfig)
    {
        //siloBuilder.AddRedisGrainStorageAsDefault("PubSubStore");
        siloBuilder.AddPersistentStreams(providerName, RedisStreamFactory.Create, null);

        siloBuilder.Services.AddSingleton<IDatabase>(sp => { return ConnectionMultiplexer.Connect(redisConfig).GetDatabase(); });

        siloBuilder.Services.AddOptions<HashRingStreamQueueMapperOptions>(providerName)
                .Configure(options =>
                {
                    options.TotalQueueCount = 8;
                });
        siloBuilder.Services.AddOptions<SimpleQueueCacheOptions>(providerName);

        return siloBuilder;
    }

    public static IClientBuilder AddRedisStream(this IClientBuilder clientBuilder, string providerName, ConfigurationOptions redisConfig)
    {
        clientBuilder.AddPersistentStreams(providerName, RedisStreamFactory.Create, null);

        clientBuilder.Services.AddSingleton<IDatabase>(sp =>
         {
             IDatabase db = ConnectionMultiplexer.Connect(redisConfig).GetDatabase();
             return db;
         });

        clientBuilder.ConfigureServices(services =>
         {
             services.AddOptions<HashRingStreamQueueMapperOptions>(providerName)
                 .Configure(options =>
                 {
                     options.TotalQueueCount = 8;
                 });

         });
        return clientBuilder;
    }
}

