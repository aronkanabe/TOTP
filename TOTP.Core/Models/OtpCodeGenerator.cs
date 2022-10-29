namespace TOTP.Core.Models;

public class OtpCodeGenerator : IOtpCodeGenerator
{
    private readonly IRedisRepository redisRepository;
    private readonly IConfiguration configuration;

    public OtpCodeGenerator(IRedisRepository redisRepository, IConfiguration configuration)
    {
        this.redisRepository = redisRepository;
        this.configuration = configuration;
        
        byte[]? secretKey = redisRepository.GetMasterKey();
        if (secretKey != null) return;
        // generate master key
        secretKey = new byte[64];
        RandomNumberGenerator.Fill(secretKey);
        redisRepository.SetMasterKey(secretKey!);
    }

    public OtpCode GenerateOtpCode(Guid userId, DateTime dateTime)
    {
        if (userId == Guid.Empty) throw new ArgumentException("Empty userId not accepted");

        var masterKey = redisRepository.GetMasterKey();
        if (masterKey == null) throw new InvalidOperationException("Master key not set!");
        
        // generate shared key for each request
        byte[] sharedKey = SHA1.HashData(masterKey.Concat(userId.ToByteArray()).ToArray());

        // calculate interval
        TimeSpan validInterval = TimeSpan.FromSeconds(int.Parse(configuration.GetSection("Otp").GetSection("validInterval").Value));
        long timeCounter = dateTime.Ticks / validInterval.Ticks;

        //generate HMAC message
        byte[] hmacMessage = HMACSHA1.HashData(sharedKey, BitConverter.GetBytes(timeCounter));

        var numberOfDigits = int.Parse(configuration.GetSection("Otp").GetSection("DigitsCount").Value);
        
        var totpCode = HmacToOtpCode(hmacMessage, numberOfDigits);
        
        return new OtpCode(totpCode.ToString());
    }

    private int HmacToOtpCode(IReadOnlyList<byte> hmacMessage, int numberOfDigits)
    {
        // dynamic truncation
        int offset = hmacMessage[19] & 0xf;
        int binCode = (hmacMessage[offset] & 0x7f) << 24
                       | (hmacMessage[offset + 1] & 0xff) << 16
                       | (hmacMessage[offset + 2] & 0xff) << 8
                       | (hmacMessage[offset + 3] & 0xff);

        // Get the last {numberOfDigits} of the generated 31 bit number
        var totpCode = binCode % (int) Math.Pow(10, numberOfDigits);
        return totpCode;
    }
}