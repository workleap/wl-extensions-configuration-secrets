using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Workleap.Extensions.Configuration.Secrets.Tests;

public class TokenCredentialProviderTests
{
    [Fact]
    public void TokenCredentialProvider_Caches_TokenCredential_Per_Environment()
    {
        var devEnvironment = new FakeHostEnvironment(Environments.Development);
        var stgEnvironment = new FakeHostEnvironment(Environments.Staging);

        var devTokenCredential1 = new TokenCredentialProvider(devEnvironment).GetTokenCredential();
        var devTokenCredential2 = new TokenCredentialProvider(devEnvironment).GetTokenCredential();
        Assert.Same(devTokenCredential1, devTokenCredential2);

        var stgTokenCredential1 = new TokenCredentialProvider(stgEnvironment).GetTokenCredential();
        var stgTokenCredential2 = new TokenCredentialProvider(stgEnvironment).GetTokenCredential();
        Assert.Same(stgTokenCredential1, stgTokenCredential2);

        Assert.NotSame(devTokenCredential1, stgTokenCredential2);
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public FakeHostEnvironment(string environmentName)
        {
            this.EnvironmentName = environmentName;
            this.ApplicationName = "Tests";
            this.ContentRootPath = AppContext.BaseDirectory;
            this.ContentRootFileProvider = new NullFileProvider();
        }

        public string EnvironmentName { get; set; }

        public string ApplicationName { get; set; }

        public string ContentRootPath { get; set; }

        public IFileProvider ContentRootFileProvider { get; set; }
    }
}