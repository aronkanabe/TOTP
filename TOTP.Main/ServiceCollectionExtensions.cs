// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Node.Infrastructure;
// using Node.Infrastructure.Configuration;
//
// namespace Node.Main;
//
// public static class ServiceCollectionExtensions
// {
//     public static void AddNodeDataContext(this IServiceCollection services, IConfiguration configuration)
//     {
//         DbContextInfo dbContextInfo = new DbContextInfo(configuration);
//         services.AddDbContext<NodeDbContext>(builder => builder.UseNpgsql(dbContextInfo.ConnectionString,
//             optionsBuilder =>
//             {
//             }));
//     }
// }