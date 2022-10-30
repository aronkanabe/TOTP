using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using TOTP.Core.Interfaces;
using TOTP.Core.Models;

namespace TOTP.UnitTests.Core.Models;

public class OtpCodeGeneratorTests
{
    private Mock<IRedisRepository> mockRedisRepository;
    private Mock<IRandomGenerator> mockRandomGenerator;
    private Mock<OtpCodeGenerator> mockOtpCodeGenerator;
    private Mock<IKeyProvider> mockKeyProvider;
    private OtpOptions otpOptions;
    private IOptions<OtpOptions> otpOptionsWrapper;

    private readonly byte[] masterKey = { 0x7e, 0xd2, 0xba, 0xad, 0x1d, 0xab, 0xa4, 0xb9, 0x22, 0x77, 0x87, 0xa2, 0xe3, 0x88, 0x04, 0x59,
        0xaa, 0xec, 0x70, 0xce, 0x89, 0xd5, 0xcc, 0xdf, 0x3f, 0x6f, 0xf4, 0xd0, 0xe8, 0xad, 0x4e, 0xbc, 0xc7, 0x67, 0x5d, 0x1a, 0xfe, 0x1d,
        0xa2, 0x35, 0x0a, 0xcb, 0x94, 0x07, 0x47, 0x12, 0x18, 0x7d, 0x7f, 0x17, 0x98, 0x74, 0xef, 0xff, 0x58, 0x97, 0x80, 0x63, 0x43, 0xa6,
        0x30, 0x19, 0x3f, 0xc9};
    
    private Guid userId = Guid.Parse("99d0f4d2-45e1-4333-81a7-f30484bb71bc");

    [SetUp]
    public void SetUp()
    {
        mockRedisRepository = new Mock<IRedisRepository>();
        mockRandomGenerator = new Mock<IRandomGenerator>();
        mockKeyProvider = new Mock<IKeyProvider>();
        otpOptions = new OtpOptions();
        otpOptionsWrapper = Options.Create(otpOptions);
        mockOtpCodeGenerator = new Mock<OtpCodeGenerator>(mockRedisRepository.Object, otpOptions, mockRandomGenerator.Object)
        {
            CallBase = true
        };
    }
    
    [Test]
    public void Constructor_WithExistingMasterKey_ShouldNotCallSetMasterKey()
    {
        // return existing master key
        mockRedisRepository.Setup(repository => repository.GetMasterKey()).Returns(masterKey);
        mockRedisRepository.Setup(repository => repository.SetMasterKey(It.IsAny<byte[]>())).Verifiable();

        new OtpCodeGenerator(otpOptionsWrapper, mockKeyProvider.Object);
        
        mockRedisRepository.Verify(repository => repository.SetMasterKey(It.IsAny<byte[]>()), Times.Never);
    }
    
    [Test]
    public void Constructor_WithMissingMasterKey_ShouldGenerateAndSetMasterKey()
    {
        // return existing master key
        mockRedisRepository.Setup(repository => repository.GetMasterKey()).Returns((byte[]?) null);
        mockRedisRepository.Setup(repository => repository.SetMasterKey(It.IsAny<byte[]>())).Verifiable();
        mockRandomGenerator.Setup(generator => generator.Fill(It.IsAny<byte[]>())).Returns(masterKey).Verifiable();

        new OtpCodeGenerator(otpOptionsWrapper, mockKeyProvider.Object);
        
        mockRedisRepository.Verify(repository => repository.SetMasterKey(It.IsAny<byte[]>()), Times.AtLeastOnce);
        mockRandomGenerator.Verify(generator => generator.Fill(It.IsAny<byte[]>()), Times.AtLeastOnce);
    }
    
    [Test]
    public void GenerateOtpCode_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // time not matters
        var dateTime = new DateTime(1000, 01, 01);
        var act = () => mockOtpCodeGenerator.Object.GenerateOtpCode(Guid.Empty, dateTime, OtpHashAlgorithm.Sha1);

        act.Should().Throw<ArgumentException>();
    }
    
    [Test]
    public void GenerateOtpCode_WithNullMasterKey_ShouldThrowInvalidOperationException()
    {
        // time not matters
        mockRedisRepository.Setup(repository => repository.GetMasterKey()).Returns((byte[]?) null);
        
        var dateTime = new DateTime(1000, 01, 01);
        var act = () => mockOtpCodeGenerator.Object.GenerateOtpCode(userId, dateTime, OtpHashAlgorithm.Sha1);

        act.Should().Throw<InvalidOperationException>();
    }
    
    [Test]
    [TestCase(6)]
    [TestCase(9)]
    public void GenerateOtpCode_WithCorrectParameters_ShouldGenerateNumberOfDigitsCode(int numberOfDigits)
    {
        mockRedisRepository.Setup(repository => repository.GetMasterKey()).Returns(masterKey);
        otpOptions.ValidInterval = TimeSpan.FromSeconds(30);
        otpOptions.DigitsCount = numberOfDigits;

        // time not matters
        var dateTime = new DateTime(1000, 01, 01);
        OtpCode result = mockOtpCodeGenerator.Object.GenerateOtpCode(userId, dateTime, OtpHashAlgorithm.Sha1);

        result.Should().BeOfType<OtpCode>();
        result.Code.Length.Should().Be(numberOfDigits);
    }
    
    [Test]
    [TestCase(6)]
    [TestCase(9)]
    public void HmacToOtpCode_WithZeroHmacMessage_ShouldLeftPadOtpCode(int numberOfDigits)
    {
        mockRedisRepository.Setup(repository => repository.GetMasterKey()).Returns(masterKey);
        otpOptions.ValidInterval = TimeSpan.FromSeconds(30);
        otpOptions.DigitsCount = numberOfDigits;
        
        // time not matters
        var hmacZero = new byte[64];
        var dateTime = new DateTime(1000, 01, 01);
        OtpCode result = mockOtpCodeGenerator.Object.GenerateOtpCode(userId, dateTime, OtpHashAlgorithm.Sha1);
    
        result.Should().BeOfType<OtpCode>();
        result.Code.Length.Should().Be(numberOfDigits);
        result.Code.Should().StartWith(new string('0', numberOfDigits));
    }
}