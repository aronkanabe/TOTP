using TOTP.Core.Models;

namespace TOTP.Core.Interfaces;

public interface IRedisRepository
{
    void SetBytes(string key, byte[] array);
    byte[]? GetBytes(string key);

    void SetString(string key, string text, TimeSpan? timeToLive);

    string? GetString(string key);

    byte[]? GetMasterKey();
    OtpCode? GetOtpCode(Guid userId, long timeWindow);
    
    void SetOtpCode(Guid userId, long timeWindow, OtpCode otpCode);
    void SetMasterKey(byte[] masterKey);
}