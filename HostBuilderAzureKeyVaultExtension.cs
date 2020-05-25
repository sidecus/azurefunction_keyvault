namespace zyin.Function
{
    using System;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureKeyVault;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Extension class for IFunctionsHostBuilder to enable KeyVault.
    /// Use app id and app secret for local development and managed identity for prod.
    /// </summary>
    public static class HostBuilderAzureKeyVaultExtensions
    {
        /// <summary>
        /// Key vault name - set in local.settings.json or app settings for prod
        /// </summary>
        private static readonly string AppSettings_KeyVaultNameKey = "KeyVaultName";

        /// <summary>
        /// Key vault app id - development only, set in user secrets.
        /// </summary>
        private static readonly string UserSecrets_KeyVaultAppIdKey = "KeyVaultAppId";

        /// <summary>
        /// Key vault app secret - development only, set in user secrets.
        /// </summary>
        private static readonly string UserSecrets_KeyVaultAppSecretKey = "KeyVaultAppSecret";

        /// <summary>
        /// Hosting environment
        /// </summary>
        /// <returns></returns>
        private static string HostingEnvironment => Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");

        /// <summary>
        /// Is the current environment a development env
        /// </summary>
        private static bool IsDevelopment => HostingEnvironment == "Development";

        /// <summary>
        /// Keyvault name
        /// </summary>
        private static string KeyVaultName => Environment.GetEnvironmentVariable(AppSettings_KeyVaultNameKey);

        /// <summary>
        /// Keyvault url
        /// </summary>
        private static string KeyVaultUrl => $"https://{KeyVaultName}.vault.azure.net/";

        /// <summary>
        /// Add Azure key vault to FunctionHostBuilder's configuration builder.
        /// Use app id and app secret for local development and managed identity for prod.
        /// </summary>
        /// <param name="IFunctionsHostBuilder">host builder</param>
        /// <returns>host builder</returns>
        public static IFunctionsHostBuilder AddAzureKeyVault(this IFunctionsHostBuilder hostBuilder)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            // Add default Azure function config first
            var defaultConfig = GetDefaultAzureFunctionConfig(hostBuilder);
            var configBuilder = new ConfigurationBuilder().AddConfiguration(defaultConfig);

            // Add keyvault as a new config provider
            if (IsDevelopment)
            {
                configBuilder.AddAzureKeyVaultServiceFromUserSecrets();
            }
            else
            {
                configBuilder.AddAzureKeyVaultFromManagedIdentity();
            }

            // Replace the configuration in DI container
            var newConfig = configBuilder.Build();
            hostBuilder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), newConfig));

            return hostBuilder;
        }

        /// <summary>
        /// Add IAzureKeyVaultService which wraps KeyVault. Use client id and client secret from user secrets to initialize keyvault (local development).
        /// </summary>
        /// <param name="configBuilder">cofig builder</param>
        /// <returns>config builder</returns>
        private static IConfigurationBuilder AddAzureKeyVaultServiceFromUserSecrets(this IConfigurationBuilder configBuilder)
        {
            if (configBuilder == null)
            {
                throw new ArgumentNullException(nameof(configBuilder));
            }

            // Load keyvault app id and app secret from user secrets
            configBuilder.AddUserSecrets<Startup>();
            var tempConfig = configBuilder.Build();
            var clientId = tempConfig[UserSecrets_KeyVaultAppIdKey];
            var clientSecret = tempConfig[UserSecrets_KeyVaultAppSecretKey];

            return configBuilder.AddAzureKeyVault(KeyVaultUrl, clientId, clientSecret);
        }

        /// <summary>
        /// Add FunctionSecretManager service which wraps KeyVault. Use Managed identity.
        /// </summary>
        /// <param name="configBuilder">config builder</param>
        /// <returns>config builder</returns>
        private static IConfigurationBuilder AddAzureKeyVaultFromManagedIdentity(this IConfigurationBuilder configBuilder)
        {
            if (configBuilder == null)
            {
                throw new ArgumentNullException(nameof(configBuilder));
            }

            // Use managed identity
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            return configBuilder.AddAzureKeyVault(KeyVaultUrl, keyVaultClient, new DefaultKeyVaultSecretManager());
        }

        /// <summary>
        /// Get base configuration builder for Function app, by adding Function app original IConfiguration as config root.
        /// This is a hack since Azure function host builder doesn't expose a way to customize ConfigurationBuilder.
        /// </summary>
        /// <param name="builder"host builder></param>
        /// <returns>configuration builder</returns>
        private static IConfiguration GetDefaultAzureFunctionConfig(IFunctionsHostBuilder builder)
        {
            return builder.Services.BuildServiceProvider().GetService<IConfiguration>();
        }
    }
}