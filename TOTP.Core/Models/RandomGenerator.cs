namespace TOTP.Core.Models;

public class RandomGenerator : IRandomGenerator
{
    public byte[] Fill(byte[] secretKey)
    {
        RandomNumberGenerator.Fill(secretKey);
        return secretKey;
    }
}