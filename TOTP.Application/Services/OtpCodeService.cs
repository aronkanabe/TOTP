using Microsoft.Extensions.Configuration;
using TOTP.Application.Interfaces;
using TOTP.Core.Interfaces;
using TOTP.Core.Models;

namespace TOTP.Application.Services;

public class OtpCodeService : IOtpCodeService
{
    private readonly IOtpCodeGenerator otpCodeGenerator;

    private readonly IRedisRepository redisRepository;
    private readonly IConfiguration configuration;

    public OtpCodeService(IOtpCodeGenerator otpCodeGenerator, IRedisRepository redisRepository, IConfiguration configuration)
    {
        this.otpCodeGenerator = otpCodeGenerator;
        this.redisRepository = redisRepository;
        this.configuration = configuration;
    }

    public OtpCode Generate(Guid userId, DateTime dateTime)
    {
        return otpCodeGenerator.GenerateOtpCode(userId, dateTime);
    }

    public bool Verify(Guid userId, DateTime dateTime, OtpCode otpCode)
    {
        var storedOtpCode = redisRepository.GetOtpCode(userId);
        if (storedOtpCode != null && otpCode.Equals(storedOtpCode)) return false;
        
        var generatedCode = otpCodeGenerator.GenerateOtpCode(userId, dateTime);
        if (!otpCode.Equals(generatedCode)) return false;
        
        var validInterval = int.Parse(configuration.GetSection("Otp").GetSection("ValidInterval").Value);
        
        redisRepository.SetOtpCode(userId, otpCode, TimeSpan.FromSeconds(validInterval));
        return true;
    }
}