using Microsoft.Extensions.Options;

namespace TOTP.Core.Models;

public class OtpCodeGenerator : IOtpCodeGenerator
{
    private readonly OtpOptions otpOptions;
    private readonly IKeyProvider keyProvider;

    public OtpCodeGenerator(IOptions<OtpOptions> otpOptions, IKeyProvider keyProvider)
    {
        this.otpOptions = otpOptions.Value;
        this.keyProvider = keyProvider;
    }

    public OtpCode GenerateOtpCode(Guid userId, DateTime dateTime, OtpHashAlgorithm otpHashAlgorithm)
    {
        if (userId == Guid.Empty) throw new ArgumentException("Empty userId not accepted");

        // generate shared key for each request
        byte[] sharedKey = keyProvider.DeriveKey(userId, otpHashAlgorithm);

        // calculate interval
        var timeCounter = CalculateTimeCounter(dateTime);

        //generate HMAC message
        var hmacAlgorithm = keyProvider.HmacAlgorithm(otpHashAlgorithm);
        hmacAlgorithm.Key = sharedKey;

        var counterBytes = BitConverter.GetBytes(timeCounter);
        if (BitConverter.IsLittleEndian)
            counterBytes = counterBytes.Reverse().ToArray();
        byte[] hmacMessage = hmacAlgorithm.ComputeHash(counterBytes);

        var numberOfDigits = otpOptions.DigitsCount;
        
        var totpCode = HmacToOtpCode(hmacMessage, numberOfDigits);
        
        return new OtpCode(totpCode);
    }

    public long CalculateTimeCounter(DateTime dateTime)
    {
        TimeSpan validInterval = otpOptions.ValidInterval;
        long timeCounter = (dateTime.Ticks - 621355968000000000L) / validInterval.Ticks;
        return timeCounter;
    }

    public static string HmacToOtpCode(IReadOnlyList<byte> hmacMessage, int numberOfDigits)
    {
        // dynamic truncation
        int offset = hmacMessage[hmacMessage.Count - 1] & 0xf;
        int binCode = (hmacMessage[offset] & 0x7f) << 24
                       | (hmacMessage[offset + 1] & 0xff) << 16
                       | (hmacMessage[offset + 2] & 0xff) << 8
                       | (hmacMessage[offset + 3] & 0xff);

        // Get the last {numberOfDigits} of the generated 31 bit number
        var totpCode = binCode % (int) Math.Pow(10, numberOfDigits);
        var totpCodeString = totpCode.ToString();
        
        if (totpCodeString.Length < numberOfDigits)
            totpCodeString = totpCodeString.PadLeft(numberOfDigits, '0');
        
        return totpCodeString;
    }
}