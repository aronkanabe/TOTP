namespace TOTP.Core.Models;

public class OtpOptions
{
    public const string Otp = "Otp";
    public TimeSpan ValidInterval { get; set; }
    
    public int DigitsCount {get; set; }
    
    public OtpHashAlgorithm HashAlgorithm { get; set; }
}