using Azure.Core;

namespace Workleap.Extensions.Configuration.Secrets;

public interface ITokenCredentialProvider
{
    TokenCredential GetTokenCredential();
}