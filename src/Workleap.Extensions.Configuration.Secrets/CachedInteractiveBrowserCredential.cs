using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

namespace Workleap.Extensions.Configuration.Secrets;

/// <summary>
/// This class should only be used for development purposes
/// </summary>
internal sealed class CachedInteractiveBrowserCredential : InteractiveBrowserCredential
{
    // InteractiveBrowserCredential sets its internal "Record" property after a token is retrieved,
    // we need its value to prevent multiple browser auth prompt after a successful one
    internal static readonly PropertyInfo InternalRecordProperty = typeof(InteractiveBrowserCredential).GetProperty("Record", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("Could not find internal property " + nameof(InteractiveBrowserCredential) + ".Result");

    // The path to the cached authentication record, not to be confused with the actual confidential oauth tokens
    // which are cached and handled internally by Azure.Identity library
    private static readonly string UserProfileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private static readonly string AuthRecordCacheDirectory = Path.Combine(UserProfileDirectory, ".gsoft");
    private static readonly string AuthRecordCachePath = Path.Combine(AuthRecordCacheDirectory, "azbrowserauthrecord.json");

    private static readonly TokenCachePersistenceOptions PersistenceOptions = new TokenCachePersistenceOptions
    {
        // Azure.Identity uses MSAL and caches tokens somewhere, the following string will be used in the cache path
        Name = "gsoft",

        // Oauth tokens must be encrypted
        UnsafeAllowUnencryptedStorage = false,
    };

    public CachedInteractiveBrowserCredential()
        : base(CreateOptions())
    {
    }

    private static InteractiveBrowserCredentialOptions CreateOptions()
    {
        Directory.CreateDirectory(AuthRecordCacheDirectory);

        var options = new InteractiveBrowserCredentialOptions
        {
            TokenCachePersistenceOptions = PersistenceOptions,
        };

        try
        {
            using var fileStream = File.OpenRead(AuthRecordCachePath);
            options.AuthenticationRecord = AuthenticationRecord.Deserialize(fileStream);
        }
        catch (FileNotFoundException)
        {
        }

        return options;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken = default)
    {
        var token = base.GetToken(requestContext, cancellationToken);
        this.PersistAuthenticationRecord();
        return token;
    }

    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken = default)
    {
        var token = await base.GetTokenAsync(requestContext, cancellationToken).ConfigureAwait(false);
        this.PersistAuthenticationRecord();
        return token;
    }

    private void PersistAuthenticationRecord()
    {
        if (InternalRecordProperty.GetValue(this) is not AuthenticationRecord authenticationRecord)
        {
            throw new InvalidOperationException("Authentication record is missing after token retrieval");
        }

        using var ms = new MemoryStream();
        authenticationRecord.Serialize(ms);
        File.WriteAllBytes(AuthRecordCachePath, ms.ToArray());
    }
}