using StackExchange.Redis;
using TOTP.Core.Interfaces;
using TOTP.Core.Models;

namespace TOTP.Infrastructure.Repositories;

public class RedisRepository : IRedisRepository
{
    private readonly IDatabase database;

    private const string MasterKey = "masterKey";

    public RedisRepository(IConnectionMultiplexer connectionMultiplexer)
    {
        database = connectionMultiplexer.GetDatabase();
    }

    public void SetBytes(string key, byte[] array)
    {
        database.StringSet(key, array);
    }

    public byte[]? GetBytes(string key)
    {
        return database.StringGet(key);
    }

    public void SetString(string key, string text, TimeSpan? timeToLive = null)
    {
        database.StringSet(key, text, timeToLive);
    }

    public string? GetString(string key)
    {
        return database.StringGet(key);
    }

    public virtual byte[]? GetMasterKey()
    {
        return GetBytes(MasterKey);
    }
    
    public OtpCode? GetOtpCode(Guid userId, long timeWindow)
    {
        string key = GetKey(userId, timeWindow);
        var storedOtpCode = GetString(key);
        return storedOtpCode == null ? null : new OtpCode(storedOtpCode);
    }

    public void SetOtpCode(Guid userId, long timeWindow, OtpCode otpCode)
    {
        var key = GetKey(userId, timeWindow);
        SetString(key, otpCode.Code);
    }

    private static string GetKey(Guid userId, long timeWindow)
    {
        string key = userId.ToString() + timeWindow;
        return key;
    }

    public void SetMasterKey(byte[] masterKey)
    {
        if (GetMasterKey() != null)
            throw new InvalidOperationException("Master could be generated only once!");
        SetBytes(MasterKey, masterKey);
    }
}