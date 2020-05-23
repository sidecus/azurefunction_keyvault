namespace Zyin.Function
{
    using System;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureKeyVault;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// extension class for IFunctionsHostBuilder to enable KeyVault (registered as singleton IAzureKeyVaultService)
    /// </summary>
    public static class HostBuilderKeyVaultExtensions
    {
        /// <summary>
        /// Add FunctionSecretManager service which wraps KeyVault based on user secrets. Use client id and client secret from user secrets for local development.
        /// </summary>
        /// <param name="IFunctionsHostBuilder">host builder</param>
        /// <param name="keyVaultUrl">keyvault url</param>
        /// <param name="keyVaultAppIdKey">User secret name for KeyVaultAppId</param>
        /// <param name="keyVaultAppSecretKey">user secret name for KeyVaultAppSecret</param>
        public static void AddAzureKeyVaultServiceFromUserSecrets(this IFunctionsHostBuilder hostBuilder, string keyVaultUrl, string keyVaultAppIdKey, string keyVaultAppSecretKey)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            if (string.IsNullOrWhiteSpace(keyVaultUrl))
            {
                throw new ArgumentNullException(nameof(keyVaultUrl));
            }

            if (string.IsNullOrWhiteSpace(keyVaultAppIdKey))
            {
                throw new ArgumentNullException(nameof(keyVaultAppIdKey));
            }

            if (string.IsNullOrWhiteSpace(keyVaultAppSecretKey))
            {
                throw new ArgumentNullException(nameof(keyVaultAppIdKey));
            }

            // Load keyvault app id and app secret from user secrets
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddUserSecrets<Startup>();
            var userSecrets = configBuilder.Build();
            var clientId = userSecrets[keyVaultAppIdKey];
            var clientSecret = userSecrets[keyVaultAppSecretKey];
            configBuilder.AddAzureKeyVault(keyVaultUrl, clientId, clientSecret);

            //Register keyvalue based secrets
            var config = configBuilder.Build();
            hostBuilder.Services.AddSingleton<IAzureKeyVaultService>(new AzureKeyVaultService(config));
        }

        /// <summary>
        /// Add FunctionSecretManager service which wraps KeyVault. Use Managed identity.
        /// </summary>
        /// <param name="IFunctionsHostBuilder">host builder</param>
        /// <param name="isDevelopment">is local development</param>
        /// <param name="keyVaultUrl">keyvault url</param>
        /// <param name="devKeyVaultAppIdKey">User secret name for KeyVaultAppId - Development only</param>
        /// <param name="devKeyVaultAppSecretKey">user secret name for KeyVaultAppSecret - Development only</param>
        public static void AddAzureKeyVaultServiceFromManagedIdentity(this IFunctionsHostBuilder hostBuilder, string keyVaultUrl)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            if (string.IsNullOrWhiteSpace(keyVaultUrl))
            {
                throw new ArgumentNullException(nameof(keyVaultUrl));
            }

            // Use managed identity
            var configBuilder = new ConfigurationBuilder();
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            configBuilder.AddAzureKeyVault(keyVaultUrl, keyVaultClient, new DefaultKeyVaultSecretManager());

            //Register keyvalue based secrets
            BuildAndAddAzureKeyVaultService(hostBuilder, configBuilder);
        }

        /// <summary>
        /// Build keyvault based config and register as IAzureKeyVaultService
        /// </summary>
        /// <param name="hostBuilder">host builder</param>
        /// <param name="configBuilder">config builder</param>
        private static void BuildAndAddAzureKeyVaultService(IFunctionsHostBuilder hostBuilder, IConfigurationBuilder configBuilder)
        {
            var config = configBuilder.Build();
            hostBuilder.Services.AddSingleton<IAzureKeyVaultService>(new AzureKeyVaultService(config));
        }
    }
}