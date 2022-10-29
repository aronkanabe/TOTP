using TOTP.API.Dto;
using TOTP.Core.Models;

namespace TOTP.API.Adapters;

public class OtpCodeAdapter : IOtpCodeAdapter
{
    public OtpCodeDto ToOtpCodeDto(OtpCode otpCode)
    {
        return new OtpCodeDto(
            otpCode.Code
        );
    }

    public OtpCode ToOtpCode(OtpCodeVerificationDto otpCodeVerificationDto)
    {
        return new OtpCode(otpCodeVerificationDto.OtpCode);
    }
}