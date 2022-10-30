using TOTP.Core.Models;

namespace TOTP.Core.Services;

public class KeyProvider : IKeyProvider
{
    public byte[] Key { get; }

    public KeyProvider(IRedisRepository redisRepository, IRandomGenerator randomGenerator)
    {
        byte[]? secretKey = redisRepository.GetMasterKey();
        if (secretKey == null)
        {
            // generate master key
            secretKey = new byte[64];
            secretKey = randomGenerator.Fill(secretKey);
            redisRepository.SetMasterKey(secretKey);
        }
        Key = secretKey;
    }

    public virtual byte[] DeriveKey(Guid userId, OtpHashAlgorithm hashAlgorithm)
    {
        // generate shared key for each request
        return HashAlgorithm(hashAlgorithm).ComputeHash(Key.Concat(userId.ToByteArray()).ToArray());
    }
    
    public HashAlgorithm HashAlgorithm(OtpHashAlgorithm hashAlgorithm)
    {
        return hashAlgorithm switch
        {
            OtpHashAlgorithm.Sha256 => SHA256.Create(),
            OtpHashAlgorithm.Sha512 => SHA512.Create(),
            _ => SHA1.Create()
        };
    }

    public HMAC HmacAlgorithm(OtpHashAlgorithm hashAlgorithm)
    {
        return hashAlgorithm switch
        {
            OtpHashAlgorithm.Sha256 => new HMACSHA256(),
            OtpHashAlgorithm.Sha512 => new HMACSHA512(),
            _ => new HMACSHA1()
        };
    }
}