using TOTP.API;
using TOTP.Main;


var apiMain = new ApiMain();
apiMain.Run(ServiceCollectionExtensions.RegisterServices);
