using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TOTP.Core.Interfaces;
using TOTP.Infrastructure.Repositories;

namespace TOTP.Main;

public static class ServiceCollectionExtensions
{
    public static void AddRedisConnectionMultiplexer(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfiguration = configuration.GetSection("Redis");
        var multiplexer = ConnectionMultiplexer.Connect(redisConfiguration.GetSection("Address").Value);
        services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        services.AddScoped<IRedisRepository, RedisRepository>();
    }
}