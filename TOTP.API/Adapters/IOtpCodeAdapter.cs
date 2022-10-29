using TOTP.API.Dto;
using TOTP.Core.Models;

namespace TOTP.API.Adapters;

public interface IOtpCodeAdapter
{
    OtpCodeDto ToOtpCodeDto(OtpCode otpCode);

    OtpCode ToOtpCode(OtpCodeVerificationDto otpCodeVerificationDto);
}