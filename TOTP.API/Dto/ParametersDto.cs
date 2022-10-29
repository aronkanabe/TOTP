namespace TOTP.API.Dto;

public class ParametersDto
{
    public Guid UserId { get; set; }
    
    public DateTime DateTime { get; set; }

    public ParametersDto(Guid userId, DateTime dateTime)
    {
        UserId = userId;
        DateTime = dateTime;
    }
}