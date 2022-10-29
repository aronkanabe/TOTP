using TOTP.Core.Interfaces;
using TOTP.Core.Models;

namespace TOTP.Application.Interfaces;

public interface IOtpCodeService
{
    OtpCode Generate(Guid userId, DateTime dateTime);

    bool Verify(Guid userId, DateTime dateTime, OtpCode otpCode);
}