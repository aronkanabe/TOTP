namespace TOTP.API.Dto;

public class OtpCodeVerificationDto : ParametersDto
{
    public string OtpCode { get; }
    public OtpCodeVerificationDto(Guid userId, DateTime dateTime, string otpCode) : base(userId, dateTime)
    {
        OtpCode = otpCode;
    }
}