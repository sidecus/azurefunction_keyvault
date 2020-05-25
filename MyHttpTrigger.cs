namespace zyin.Function
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// class demoing HttpTrigger with keyvault support
    /// </summary>
    public class MyHttpTrigger
    {
        /// <summary>
        /// Reference to the azure keyvault service
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Reference to app config
        /// </summary>
        private readonly AppConfig appConfig;

        /// <summary>
        /// Initializes a new MyHttpTriggerFunction class.
        /// It takes IAzureKeyVaultService from the DI container.
        /// </summary>
        /// <param name="keyVaultService"></param>
        public MyHttpTrigger(IConfiguration configuration, IOptions<AppConfig> appConfigOptions)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.appConfig = appConfigOptions?.Value ?? throw new ArgumentNullException(nameof(appConfigOptions));
        }

        /// <summary>
        /// An Azure function endpoint to dump secrets from Azure keyvault.
        /// This is for demo purpose, don't do this for real secrets or in Production.
        /// 1. We use ActionResult<T> instead of IActionResult for better return type checking.
        /// 2. Use IAzureKeyVaultService to get secret value.
        /// </summary>
        /// <param name="req">http request</param>
        /// <param name="secretName">secret name</param>
        /// <returns>message contains the secret value</returns>
        [FunctionName("ShowKeyVaultSecret")]
        public ActionResult<string> ShowKeyVaultSecret(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "vault/{secretName}")] HttpRequest req,
            string secretName,
            ILogger log)
        {
            var secretValue = this.configuration.GetValue<string>(secretName);
            var message = secretValue != null ? $"Hush, this is our secret - {secretName} : {secretValue}" : $"Secret {secretName} doesn't exist in keyvault.";
            return message;
        }

        /// <summary>
        /// An Azure function endpoint to render AppConfig
        /// </summary>
        /// <param name="req">http request</param>
        /// <returns>AppConfig</returns>
        [FunctionName("ShowAppConfig")]
        public ActionResult<AppConfig> ShowAppConfig(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "appconfig")] HttpRequest req,
            ILogger log)
        {
            return this.appConfig;
        }
    }
}
