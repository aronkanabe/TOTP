using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TOTP.API.Adapters;
using TOTP.Application.Interfaces;
using TOTP.Application.Services;
using TOTP.Core.Interfaces;
using TOTP.Core.Models;
using TOTP.Core.Services;
using TOTP.Infrastructure.Repositories;

namespace TOTP.Main;

public static class ServiceCollectionExtensions
{
    public static void RegisterServices(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<OtpOptions>(configuration.GetSection(OtpOptions.Otp));
        serviceCollection.AddRedisConnectionMultiplexer(configuration);
        serviceCollection.AddScoped<IOtpCodeGenerator, OtpCodeGenerator>();
        serviceCollection.AddScoped<IOtpCodeService, OtpCodeService>();
        serviceCollection.AddScoped<IOtpCodeAdapter, OtpCodeAdapter>();
        serviceCollection.AddScoped<IKeyProvider, KeyProvider>();
        serviceCollection.AddTransient<IRandomGenerator, RandomGenerator>();
    }
    
    public static void AddRedisConnectionMultiplexer(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfiguration = configuration.GetSection("Redis");
        var multiplexer = ConnectionMultiplexer.Connect(redisConfiguration.GetSection("Address").Value);
        services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        services.AddScoped<IRedisRepository, RedisRepository>();
    }
}