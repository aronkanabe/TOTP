using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using TOTP.API.Adapters;
using TOTP.API.Dto;
using TOTP.Application.Interfaces;
using TOTP.Core.Models;

namespace TOTP.API.Controllers;

[ApiController]
[Route("/otp")]
public class OtpController : ControllerBase
{
    private readonly IOtpCodeAdapter otpCodeAdapter;
    private readonly IOtpCodeService otpCodeService;

    public OtpController(IOtpCodeAdapter otpCodeAdapter, IOtpCodeService otpCodeService)
    {
        this.otpCodeAdapter = otpCodeAdapter;
        this.otpCodeService = otpCodeService;
    }

    [HttpPost("/generate")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<OtpCodeDto> GenerateOtpCode([FromBody]ParametersDto parametersDto)
    {
        OtpCodeDto otpCodeDto;
        try
        {
            OtpCode otpCode = otpCodeService.Generate(parametersDto.UserId, parametersDto.DateTime);
            otpCodeDto = otpCodeAdapter.ToOtpCodeDto(otpCode);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return Ok(otpCodeDto);
    }
    
    [HttpPost("/verify")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult CreateNode([FromBody] OtpCodeVerificationDto otpCodeVerificationDto)
    {
        try
        {
            OtpCode otpCode = otpCodeAdapter.ToOtpCode(otpCodeVerificationDto);
            bool codeAccepted =
                otpCodeService.Verify(otpCodeVerificationDto.UserId, otpCodeVerificationDto.DateTime, otpCode);
            if (codeAccepted)
                return Ok("Accepted");
        }
        catch (ArgumentException argumentException)
        {
            return BadRequest(argumentException.Message);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return Unauthorized("Code is invalid");
    }
}