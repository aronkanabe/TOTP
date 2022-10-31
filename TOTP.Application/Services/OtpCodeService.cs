using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TOTP.Application.Interfaces;
using TOTP.Core.Interfaces;
using TOTP.Core.Models;

namespace TOTP.Application.Services;

public class OtpCodeService : IOtpCodeService
{
    private readonly IOtpCodeGenerator otpCodeGenerator;

    private readonly IRedisRepository redisRepository;
    private readonly IConfiguration configuration;
    private readonly OtpOptions otpOptions;

    public OtpCodeService(IOtpCodeGenerator otpCodeGenerator, IRedisRepository redisRepository, IConfiguration configuration, IOptions<OtpOptions> otpOptions)
    {
        this.otpCodeGenerator = otpCodeGenerator;
        this.redisRepository = redisRepository;
        this.configuration = configuration;
        this.otpOptions = otpOptions.Value;
    }

    public OtpCode Generate(Guid userId, DateTime dateTime, OtpHashAlgorithm? otpHashAlgorithm = null)
    {
        return otpCodeGenerator.GenerateOtpCode(userId, dateTime,otpHashAlgorithm ?? otpOptions.HashAlgorithm);
    }

    public bool Verify(Guid userId, DateTime dateTime, OtpCode otpCode, OtpHashAlgorithm? otpHashAlgorithm = null)
    {
        var timeWindow = otpCodeGenerator.CalculateTimeCounter(dateTime);
        var storedOtpCode = redisRepository.GetOtpCode(userId, timeWindow);
        if (storedOtpCode != null && otpCode.Equals(storedOtpCode)) return false;
        
        var generatedCode = otpCodeGenerator.GenerateOtpCode(userId, dateTime, otpHashAlgorithm ??  otpOptions.HashAlgorithm);
        if (!otpCode.Equals(generatedCode)) return false;
        
        var validInterval = otpOptions.ValidInterval;
        
        redisRepository.SetOtpCode(userId, timeWindow, otpCode);
        return true;
    }
}