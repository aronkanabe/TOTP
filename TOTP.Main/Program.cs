
using Microsoft.Extensions.DependencyInjection;
using TOTP.API;
using TOTP.API.Adapters;
using TOTP.Application.Interfaces;
using TOTP.Application.Services;
using TOTP.Core.Interfaces;
using TOTP.Core.Models;
using TOTP.Main;

var apiMain = new ApiMain();
apiMain.Run((serviceCollection, configuration) =>
{
    serviceCollection.AddRedisConnectionMultiplexer(configuration);
    serviceCollection.AddScoped<IOtpCodeGenerator, OtpCodeGenerator>();
    serviceCollection.AddScoped<IOtpCodeService, OtpCodeService>();
    serviceCollection.AddScoped<IOtpCodeAdapter, OtpCodeAdapter>();
});

// // input/ API controller
// Guid userId = Guid.Parse("3fc5f91b-dbf1-40f6-b8dd-e20794bef157");
// DateTime dateTime = new DateTime(2022, 10, 29);
//
//
// // Startup generate or read in stored in redis
// byte[] secretKey = new byte[64];
// // generate master key
// RandomNumberGenerator.Fill(secretKey);
//
//
// // generate shared key for each request
// byte[] sharedKey = SHA1.HashData(secretKey.Concat(userId.ToByteArray()).ToArray());
//
// // calculate interval
// int validInterval = 30; // configuration
// long timeCounter = dateTime.Ticks / validInterval;
//
// //generate HMAC message
// byte[] hmacMessage = HMACSHA1.HashData(sharedKey, BitConverter.GetBytes(timeCounter));
//
//
// // dynamic truncation
// int offset = hmacMessage[19] & 0xf;
// int bin_code = (hmacMessage[offset] & 0x7f) << 24
//                | (hmacMessage[offset + 1] & 0xff) << 16
//                | (hmacMessage[offset + 2] & 0xff) << 8
//                | (hmacMessage[offset + 3] & 0xff);
//
// // get TOTP digits
// int numberOfDigits = 6; // configuration
// int totpCode = bin_code % (int)Math.Pow(10, numberOfDigits);
//
// Console.WriteLine(totpCode);
