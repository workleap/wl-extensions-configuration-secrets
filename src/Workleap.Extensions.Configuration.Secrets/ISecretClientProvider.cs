using System;
using Azure.Security.KeyVault.Secrets;

namespace Workleap.Extensions.Configuration.Secrets;

public interface ISecretClientProvider
{
    SecretClient GetSecretClient(KeyVaultKind keyVaultKind, SecretClientOptions? options = null);

    SecretClient GetSecretClient(Uri keyVaultUri, SecretClientOptions? options = null);

    SecretClient GetSecretClient(string configurationKey, SecretClientOptions? options = null);
}