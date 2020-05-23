namespace zyin.Function
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// class demoing HttpTrigger with keyvault support
    /// </summary>
    public class MyHttpTrigger
    {
        /// <summary>
        /// Reference to the azure keyvault service
        /// </summary>
        private readonly IAzureKeyVaultService keyVaultService;

        /// <summary>
        /// Initializes a new MyHttpTriggerFunction class.
        /// It takes IAzureKeyVaultService from the DI container.
        /// </summary>
        /// <param name="keyVaultService"></param>
        public MyHttpTrigger(IAzureKeyVaultService keyVaultService)
        {
            this.keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
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
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "vault/{secretName}")] HttpRequest req,
            string secretName,
            ILogger log)
        {
            var secretValue = this.keyVaultService.GetSecret<string>(secretName);
            var message = secretValue != null ? $"Hush, this is our secret - {secretName} : {secretValue}" : $"Secret {secretName} doesn't exist in keyvault.";
            return message;
        }
    }
}
