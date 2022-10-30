using TOTP.Core.Interfaces;
using TOTP.Core.Models;

namespace TOTP.Application.Interfaces;

public interface IOtpCodeService
{
    OtpCode Generate(Guid userId, DateTime dateTime, OtpHashAlgorithm? otpHashAlgorithm = null);

    bool Verify(Guid userId, DateTime dateTime, OtpCode otpCode, OtpHashAlgorithm? otpHashAlgorithm = null);
}