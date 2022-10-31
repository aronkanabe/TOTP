using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using TOTP.API.Controllers;
using TOTP.Application.Interfaces;
using TOTP.Application.Services;
using TOTP.Core.Interfaces;
using TOTP.Core.Models;
using TOTP.Core.Services;
using TOTP.Infrastructure.Repositories;
using TOTP.IntegrationTests.Utils;
using TOTP.Main;

namespace TOTP.IntegrationTests.API.Controllers;

public class OtpControllerTests
{
     private IOtpCodeService otpCodeService;

     private Mock<KeyProvider> mockKeyProvider;

     private Guid userId;
     
    [SetUp]
    public void SetUp()
    {
        userId = Guid.Parse("99d0f4d2-45e1-4333-81a7-f30484bb71bc");
        
        var serviceCollection = new ServiceCollection();
        var configuration = TestConfiguration.GetConfiguration();
        configuration.GetSection(OtpOptions.Otp).GetSection(nameof(OtpOptions.DigitsCount)).Value = "8";
        serviceCollection.AddSingleton(configuration);
        ServiceCollectionExtensions.RegisterServices(serviceCollection, configuration);

        serviceCollection.AddScoped<IKeyProvider>((serviceProvider) =>
        {
            mockKeyProvider = new Mock<KeyProvider>(
                serviceProvider.GetService<IRedisRepository>(),
                serviceProvider.GetService<IRandomGenerator>()
                );
            return mockKeyProvider.Object;
        });
        
        // serviceCollection.AddScoped<IRedisRepository>((serviceProvider) =>
        // {
        //     var mockRedisRepository = new Mock<RedisRepository>(serviceProvider.GetService<IConnectionMultiplexer>());
        //     mockRedisRepository.CallBase = true;
        //     mockRedisRepository.Setup(repository => repository.GetMasterKey())
        //         .Returns(Encoding.ASCII.GetBytes("12345678901234567890"));
        //     return mockRedisRepository.Object;
        // });
        
        var serviceProvider = serviceCollection.BuildServiceProvider();

        otpCodeService = serviceProvider.GetService<IOtpCodeService>() ?? throw new InvalidOperationException();
    }
    
    [Test]
    [TestCase("12345678901234567890", 59, "94287082", OtpHashAlgorithm.Sha1)]
    [TestCase("12345678901234567890123456789012", 59, "46119246", OtpHashAlgorithm.Sha256)]
    [TestCase("1234567890123456789012345678901234567890123456789012345678901234", 59, "90693936", OtpHashAlgorithm.Sha512)]
    
    [TestCase("12345678901234567890", 1111111109, "07081804", OtpHashAlgorithm.Sha1)]
    [TestCase("12345678901234567890123456789012", 1111111109, "68084774", OtpHashAlgorithm.Sha256)]
    [TestCase("1234567890123456789012345678901234567890123456789012345678901234", 1111111109, "25091201", OtpHashAlgorithm.Sha512)]
    
    [TestCase("12345678901234567890", 1111111111, "14050471", OtpHashAlgorithm.Sha1)]
    [TestCase("12345678901234567890123456789012", 1111111111, "67062674", OtpHashAlgorithm.Sha256)]
    [TestCase("1234567890123456789012345678901234567890123456789012345678901234", 1111111111, "99943326", OtpHashAlgorithm.Sha512)]
    
    [TestCase("12345678901234567890", 1234567890, "89005924", OtpHashAlgorithm.Sha1)]
    [TestCase("12345678901234567890123456789012", 1234567890, "91819424", OtpHashAlgorithm.Sha256)]
    [TestCase("1234567890123456789012345678901234567890123456789012345678901234", 1234567890, "93441116", OtpHashAlgorithm.Sha512)]
    
    [TestCase("12345678901234567890", 2000000000, "69279037", OtpHashAlgorithm.Sha1)]
    [TestCase("12345678901234567890123456789012", 2000000000, "90698825", OtpHashAlgorithm.Sha256)]
    [TestCase("1234567890123456789012345678901234567890123456789012345678901234", 2000000000, "38618901", OtpHashAlgorithm.Sha512)]
    
    [TestCase("12345678901234567890", 20000000000, "65353130", OtpHashAlgorithm.Sha1)]
    [TestCase("12345678901234567890123456789012", 20000000000, "77737706", OtpHashAlgorithm.Sha256)]
    [TestCase("1234567890123456789012345678901234567890123456789012345678901234", 20000000000, "47863826", OtpHashAlgorithm.Sha512)]
    public void Rfc6238_TestVectors(string seed, long time, string expectedOtp, OtpHashAlgorithm hashAlgorithm)
    {
        mockKeyProvider.Setup(keyProvider => keyProvider.DeriveKey(It.IsAny<Guid>(), It.IsAny<OtpHashAlgorithm>()))
            .Returns(Encoding.UTF8.GetBytes(seed));
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(time);
        var generatedOtpCode = otpCodeService.Generate(userId, dateTime, hashAlgorithm);

        generatedOtpCode.Code.Should().BeEquivalentTo(expectedOtp);
    }
}