using System.Security.Cryptography;

namespace TOTP.UnitTests.Core.Models;

public class VirtualHMAC : HMAC
{
    public virtual byte[] ComputeHash(byte[] buffer)
    {
       return base.ComputeHash(buffer);
    }
}