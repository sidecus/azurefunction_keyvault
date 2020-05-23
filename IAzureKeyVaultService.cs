namespace zyin.Function
{
    using System;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// IAzureKeyVaultService interface
    /// </summary>
    public interface IAzureKeyVaultService
    {
        /// <summary>
        /// Get one secret
        /// </summary>
        /// <param name="secretName">secret name</param>
        /// <typeparam name="T">secret type</typeparam>
        /// <returns>secret value</returns>
        T GetSecret<T>(string secretName);
    }

    /// <summary>
    /// Class to help retrieve keyvault secrets
    /// </summary>
    public class AzureKeyVaultService : IAzureKeyVaultService
    {
        /// <summary>
        /// Standard .net core IConfiguration object which has KeyVault access
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new FunctionSecretConfig
        /// </summary>
        /// <param name="configuration"></param>
        public AzureKeyVaultService(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Get one secret
        /// </summary>
        /// <param name="secretName">secret name</param>
        /// <typeparam name="T">secret type</typeparam>
        /// <returns>secret value</returns>
        public T GetSecret<T>(string secretName)
        {
            if (string.IsNullOrWhiteSpace(secretName))
            {
                throw new ArgumentNullException(nameof(secretName));
            }

            return this.configuration.GetValue<T>(secretName);
        }
    }
}