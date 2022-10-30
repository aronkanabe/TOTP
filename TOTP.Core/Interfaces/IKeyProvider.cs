using TOTP.Core.Models;

namespace TOTP.Core.Interfaces;

public interface IKeyProvider
{
    byte[] Key { get; }
    HashAlgorithm HashAlgorithm(OtpHashAlgorithm hashAlgorithm);
    HMAC HmacAlgorithm(OtpHashAlgorithm hashAlgorithm);
    byte[] DeriveKey(Guid userId, OtpHashAlgorithm hashAlgorithm);
}