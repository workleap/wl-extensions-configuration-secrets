using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Hosting;

namespace Workleap.Extensions.Configuration.Secrets;

public sealed class TokenCredentialProvider : ITokenCredentialProvider
{
    // Quote from a Microsoft Azure SDK developer:
    // "The only reason to intentionally use a different instance of the credential is if you intend to discover a different credential in the chain.
    // So generally speaking, it's better to use a singleton credential."
    // See https://github.com/Azure/azure-sdk-for-net/issues/31202#issuecomment-1284543516
    private static readonly ConcurrentDictionary<string, TokenCredential> CachedTokenCredentials = new(StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> AzureCliCompatibleEnvironments = new(StringComparer.OrdinalIgnoreCase)
    {
        // Mostly local, development, tests environments or any environment where a developer could use Azure CLI
        "Local", "LocalDocker", "Test", "Tests", "CI", Environments.Development, "DevelopmentDocker",
    };

    private static readonly string[] KnownContinuousIntegrationEnvironmentVariables = {
        "SYSTEM_TEAMFOUNDATIONCOLLECTIONURI", // Azure Pipelines
        "GITHUB_ACTIONS", // GitHub Actions
        "TEAMCITY", // TeamCity
    };

    private readonly IHostEnvironment _environment;

    public TokenCredentialProvider(IHostEnvironment environment)
    {
        this._environment = environment;
    }

    private static bool IsRunningOnBuildAgent => KnownContinuousIntegrationEnvironmentVariables
        .Any(x => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(x)));

    public TokenCredential GetTokenCredential()
    {
        return CachedTokenCredentials.GetOrAdd(this._environment.EnvironmentName, GetTokenCredentialInternal);
    }

    private static TokenCredential GetTokenCredentialInternal(string environmentName)
    {
        if (AzureCliCompatibleEnvironments.Contains(environmentName))
        {
            return GetAzureCliCompatibleTokenCredential();
        }

        // See https://docs.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet
        return new DefaultAzureCredential();
    }

    private static TokenCredential GetAzureCliCompatibleTokenCredential()
    {
        // Azure CLI does not work when Fiddler is active so we need to use an interactive authentication method instead
        if (FiddlerProxyDetector.IsFiddlerActive())
        {
            return new CachedInteractiveBrowserCredential();
        }

        // When Fiddler is not active, if running on a Build Agent, use DefaultAzureCredential in order to try with Managed Identity first
        // if running locally, try to use AzureCliCredential because it's usually faster than DefaultAzureCredential on startup
        return IsRunningOnBuildAgent
            ? new DefaultAzureCredential()
            : new ChainedTokenCredential(new AzureCliCredential(), new DefaultAzureCredential());
    }
}