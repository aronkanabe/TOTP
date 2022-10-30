namespace TOTP.Core.Interfaces;

public interface IRandomGenerator
{
    byte[] Fill(byte[] secretKey);
}