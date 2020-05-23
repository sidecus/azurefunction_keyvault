namespace Zyin.Function
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class HttpTrigger
    {
        private static readonly string SecretName = "MySecret";
        private readonly IAzureKeyVaultService keyVaultService;

        public HttpTrigger(IAzureKeyVaultService keyVaultService)
        {
            this.keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
        }

        [FunctionName("ShowMySecret")]
        public ActionResult<string> ShowSecret(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var secretValue = this.keyVaultService.GetSecret<string>(SecretName);
            return $"Hush, this is {SecretName}: {secretValue}";
        }
    }
}
