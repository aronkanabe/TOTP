using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TOTP.API.Adapters;
using TOTP.API.Controllers;
using TOTP.API.Dto;
using TOTP.Application.Interfaces;
using TOTP.Core.Models;

namespace TOTP.UnitTests.API.Controllers;

public class TopologyControllerTests
{
    private OtpController otpController;
    private Mock<IOtpCodeService> mockOtpCodeGenerator;
    private Mock<IOtpCodeAdapter> mockOtpAdaper;

    [SetUp]
    public void SetUp()
    {
        mockOtpAdaper = new Mock<IOtpCodeAdapter>();
        mockOtpCodeGenerator = new Mock<IOtpCodeService>();
        otpController = new OtpController(mockOtpAdaper.Object, mockOtpCodeGenerator.Object);
    }
    
    [Test]
    public void GenerateOtpCode_WithArgumentExceptionInOtpCodeGenerator_ShouldReturnBadRequest()
    {
        mockOtpCodeGenerator.Setup(generator => generator.Generate(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .Throws<ArgumentException>();
        
        // the actual time is not important
        var dateTime = new DateTime(2022, 10, 29);

        var parametersDto = new ParametersDto(
            Guid.NewGuid(),
            dateTime
        );
        
        var result = otpController.GenerateOtpCode(parametersDto);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public void GenerateOtpCode_WithArgumentExceptionInAdapter_ShouldReturnBadRequest()
    {
        mockOtpAdaper.Setup(adapter => adapter.ToOtpCodeDto(It.IsAny<OtpCode>()))
            .Throws<ArgumentException>();

        // the actual time is not important
        var dateTime = new DateTime(2022, 10, 29);

        var parametersDto = new ParametersDto(
            Guid.NewGuid(),
            dateTime
        );

        var result = otpController.GenerateOtpCode(parametersDto);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public void GenerateOtpCode_WithCorrectGeneratedCode_ShouldReturnBadRequest()
    {
        var otpCode = "123456";
        var returnOtpCodeDto = new OtpCodeDto(otpCode);
        mockOtpAdaper.Setup(adapter => adapter.ToOtpCodeDto(It.IsAny<OtpCode>()))
            .Returns(returnOtpCodeDto);
        
        // the actual time is not important
        var dateTime = new DateTime(2022, 10, 29);

        var parametersDto = new ParametersDto(
            Guid.NewGuid(),
            dateTime
        );

        var result = otpController.GenerateOtpCode(parametersDto);
        result.Result.Should().BeOfType<OkObjectResult>();
        var okObjectResult = result.Result.As<OkObjectResult>();
        okObjectResult.Value.Should().NotBeNull();
        okObjectResult.Value.Should().BeEquivalentTo(returnOtpCodeDto);
    }
}