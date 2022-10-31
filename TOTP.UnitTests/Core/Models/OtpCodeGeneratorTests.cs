using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using TOTP.Core.Interfaces;
using TOTP.Core.Models;

namespace TOTP.UnitTests.Core.Models;

public class OtpCodeGeneratorTests
{
    private Mock<OtpCodeGenerator> mockOtpCodeGenerator;
    private Mock<IKeyProvider> mockKeyProvider;
    private OtpOptions otpOptions;
    private IOptions<OtpOptions> otpOptionsWrapper;

    private readonly byte[] masterKey = { 0x7e, 0xd2, 0xba, 0xad, 0x1d, 0xab, 0xa4, 0xb9, 0x22, 0x77, 0x87, 0xa2, 0xe3, 0x88, 0x04, 0x59,
        0xaa, 0xec, 0x70, 0xce, 0x89, 0xd5, 0xcc, 0xdf, 0x3f, 0x6f, 0xf4, 0xd0, 0xe8, 0xad, 0x4e, 0xbc, 0xc7, 0x67, 0x5d, 0x1a, 0xfe, 0x1d,
        0xa2, 0x35, 0x0a, 0xcb, 0x94, 0x07, 0x47, 0x12, 0x18, 0x7d, 0x7f, 0x17, 0x98, 0x74, 0xef, 0xff, 0x58, 0x97, 0x80, 0x63, 0x43, 0xa6,
        0x30, 0x19, 0x3f, 0xc9};
    
    private Guid userId = Guid.Parse("99d0f4d2-45e1-4333-81a7-f30484bb71bc");
    private DateTime unixEpoch;

    [SetUp]
    public void SetUp()
    {
        unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        mockKeyProvider = new Mock<IKeyProvider>();
        otpOptions = new OtpOptions();
        otpOptionsWrapper = Options.Create(otpOptions);
        mockOtpCodeGenerator = new Mock<OtpCodeGenerator>(otpOptionsWrapper, mockKeyProvider.Object)
        {
            CallBase = true
        };
        mockKeyProvider.Setup(keyProvider => keyProvider.Key).Returns(masterKey);
        mockKeyProvider.Setup(keyProvider => keyProvider.DeriveKey(It.IsAny<Guid>(), It.IsAny<OtpHashAlgorithm>()))
            .Returns(Encoding.UTF8.GetBytes("12345678901234567890"));
        mockKeyProvider.Setup(keyProvider => keyProvider.HmacAlgorithm(It.IsAny<OtpHashAlgorithm>()))
            .Returns(new HMACSHA1());
    }

    [Test]
    public void GenerateOtpCode_WithEmptyUserId_ShouldThrowArgumentException()
    {
        var act = () => mockOtpCodeGenerator.Object.GenerateOtpCode(Guid.Empty, unixEpoch, OtpHashAlgorithm.Sha1);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    [TestCase(6)]
    [TestCase(9)]
    public void GenerateOtpCode_WithCorrectParameters_ShouldGenerateNumberOfDigitsCode(int numberOfDigits)
    {
        otpOptions.ValidInterval = TimeSpan.FromSeconds(30);
        otpOptions.DigitsCount = numberOfDigits;
        
        OtpCode result = mockOtpCodeGenerator.Object.GenerateOtpCode(userId, unixEpoch, OtpHashAlgorithm.Sha1);

        result.Should().BeOfType<OtpCode>();
        result.Code.Length.Should().Be(numberOfDigits);
    }
    
    [Test]
    [TestCase(6, 20)]
    [TestCase(9, 20)]
    [TestCase(6, 32)]
    [TestCase(9, 32)]
    [TestCase(6, 64)]
    [TestCase(9, 64)]
    public void HmacToOtpCode_WithZeroHmacMessage_ShouldLeftPadOtpCode(int numberOfDigits, int hashSize)
    {
        otpOptions.ValidInterval = TimeSpan.FromSeconds(30);
        otpOptions.DigitsCount = numberOfDigits;

        var hmacZero = new byte[hashSize];

        var result = OtpCodeGenerator.HmacToOtpCode(hmacZero, numberOfDigits);
    
        result.Should().BeOfType<string>();
        result.Length.Should().Be(numberOfDigits);
        result.Should().StartWith(new string('0', numberOfDigits));
    }
}