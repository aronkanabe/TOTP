using TOTP.Core.Models;

namespace TOTP.Core.Interfaces;

public interface IOtpCodeGenerator
{
    OtpCode GenerateOtpCode(Guid userId, DateTime dateTime);
}