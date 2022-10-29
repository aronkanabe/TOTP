namespace TOTP.API.Dto;

public class OtpCodeDto
{
    public OtpCodeDto(string otpCode)
    {
        OtpCode = otpCode;
    }

    public string OtpCode { get; set; }
}