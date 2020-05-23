using Microsoft.Azure.Functions.Extensions.DependencyInjection;
[assembly: FunctionsStartup(typeof(zyin.Function.Startup))]

namespace zyin.Function
{
    using System;
    
    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup : FunctionsStartup
    {
        private static readonly string Settings_KeyVaultNameKey = "KeyVaultName";
        private static readonly string UserSecrets_KeyVaultAppIdKey = "KeyVaultAppId";
        private static readonly string UserSecrets_KeyVaultAppSecretKey = "KeyVaultAppSecret";

        /// <summary>
        /// Hosting environment
        /// </summary>
        /// <returns></returns>
        private string hostingEnvironment => Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");

        /// <summary>
        /// Is the current environment a development env
        /// </summary>
        private bool isDevelopment => this.hostingEnvironment == "Development";

        /// <summary>
        /// Keyvault name
        /// </summary>
        private string keyVaultName => Environment.GetEnvironmentVariable(Settings_KeyVaultNameKey);

        /// <summary>
        /// Keyvault url
        /// </summary>
        private string keyVaultUrl => $"https://{this.keyVaultName}.vault.azure.net/";

        /// <summary>
        /// Configure services
        /// </summary>
        /// <param name="builder">host builder</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (this.isDevelopment)
            {
                // Add IAzureKeyVaultService from user secrets
                builder.AddAzureKeyVaultServiceFromUserSecrets(this.keyVaultUrl, UserSecrets_KeyVaultAppIdKey, UserSecrets_KeyVaultAppSecretKey);
            }
            else
            {
                // Add IAzureKeyVaultService from managed identity
                builder.AddAzureKeyVaultServiceFromManagedIdentity(this.keyVaultUrl);
            }
        }
    }
}