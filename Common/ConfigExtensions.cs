using System.Linq;
using System.Net;
using System.Security.Authentication;
using Microsoft.Extensions.Configuration;
using Orleans.Configuration;
using StackExchange.Redis;

namespace Common.Extensions;

public static class ConfigurationExt
{
    public static ConfigurationOptions GetRedisConfigurationOptions(this IConfiguration Configuration)
    {
        var serversConfig = Configuration["Redis:Servers"];
        var endpoints = new EndPointCollection();

        if (string.IsNullOrWhiteSpace(serversConfig))
        {
            // Se não definido, adiciona localhost
            endpoints.Add("localhost");
        }
        else
        {
            serversConfig.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList()
                .ForEach(x => endpoints.Add(x));
        }

        var configs = new ConfigurationOptions()
        {
            EndPoints = endpoints,
            SyncTimeout = 10000,
            ConnectRetry = 3,
            Password = string.IsNullOrEmpty(Configuration["Redis:Password"]) ? null : Configuration["Redis:Password"]
        };

        if (Configuration.GetValue<bool>("Redis:UseSSL"))
        {
            configs.Ssl = true;
            configs.SslProtocols = SslProtocols.Tls12;
            configs.AbortOnConnectFail = false;
        }

        return configs;
    }
}
