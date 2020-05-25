using Microsoft.Azure.Functions.Extensions.DependencyInjection;
[assembly: FunctionsStartup(typeof(zyin.Function.Startup))]

namespace zyin.Function
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Configure services
        /// </summary>
        /// <param name="builder">host builder</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            
            // Add azure keyvault
            builder.AddFunctionAzureKeyVault();
            
            // Inject IOptions pattern
            builder.Services
                .AddOptions<AppConfig>()
                .Configure<IConfiguration>((appConfig, configuration) =>
                {
                    configuration.GetSection("AppConfig").Bind(appConfig);
                });
        }
    }
}