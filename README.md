# azurefunction_keyvault
.Net core Azure function with UserSecret and KeyVault support.
```HostBuilderKeyVaultExtensions``` contains the extension to use .net core user secrets and keyvault from local env as well as using managed identities.

This can be useful if you have many secrets in your project. The common use case is to share a local.setting.json with each developer. But it can get outdated soon and cause confusions.

With this you can setup a Azure keyvault with just dev settings/values and let all dev machine to connect to that. Each developer only need to set two user secrets locally - other dev secrets are all auto populated and it'll never get outdated for all your team members.