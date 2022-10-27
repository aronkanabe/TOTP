using System.Security.Cryptography;
// using Node.API;

// var apiMain = new ApiMain();
// apiMain.Run((serviceCollection, configuration) =>
// {
//     serviceCollection.AddNodeDataContext(configuration);
//     
//     serviceCollection.AddScoped<IBaseNodeRepository, BaseNodeRepository>();
//     serviceCollection.AddScoped<ITopologyRepository, TopologyRepository>();
//
//     serviceCollection.AddScoped<INodeAdapter, NodeAdapter>();
//     serviceCollection.AddScoped<ITopologyAdapter, TopologyAdapter>();
//     
//     serviceCollection.AddScoped<IGenericNodeFactory, GenericGenericNodeFactory>();
//     serviceCollection.AddScoped<INodeService, NodeService>();
//     serviceCollection.AddScoped<ITopologyService, TopologyService>();
// });

// input/ API controller
Guid userId = Guid.NewGuid();
DateTime dateTime = DateTime.Today;


// Startup generate or read in stored in redis
byte[] secretKey = new byte[64];
// generate master key
RandomNumberGenerator.Fill(secretKey);


// generate shared key for each request
byte[] sharedKey = SHA512.HashData(secretKey.Concat(userId.ToByteArray()).ToArray());

// calculate interval
int validInterval = 30;
long timeCounter = dateTime.Ticks / validInterval;

//generate HMAC message
byte[] hmacMessage = HMACSHA512.HashData(sharedKey, BitConverter.GetBytes(timeCounter));


// dynamic truncation
int offset = hmacMessage[19] & 0xf;
int bin_code = (hmacMessage[offset] & 0x7f) << 24
               | (hmacMessage[offset + 1] & 0xff) << 16
               | (hmacMessage[offset + 2] & 0xff) << 8
               | (hmacMessage[offset + 3] & 0xff);

// get TOTP digits
int numberOfDigits = 6;
int totpCode = bin_code % (int)Math.Pow(10, numberOfDigits);

Console.WriteLine(totpCode);
