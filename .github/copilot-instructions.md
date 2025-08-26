# Workleap.Extensions.Configuration.Secrets

A .NET library for integrating configuration values from Azure Key Vault Secrets using environment-appropriate Azure credentials. This package supports ASP.NET Core applications and provides `ITokenCredentialProvider` and `ISecretClientProvider` services for accessing Azure credentials and SecretClient instances.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Prerequisites
- Install .NET SDK 9.0.304 exactly (required by global.json):
  ```bash
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 9.0.304
  export PATH="$HOME/.dotnet:$PATH"
  ```
- Install .NET 8.0 runtime for testing:
  ```bash
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --runtime dotnet --version 8.0.10
  ```

### Build, Test, and Package
- Development workflow (Debug builds work reliably):
  ```bash
  cd src
  dotnet clean
  dotnet restore
  dotnet build
  dotnet test
  dotnet pack -c Debug -o .output
  ```

- Full Release build (can have GitVersion issues):
  ```bash
  pwsh ./Build.ps1          # NEVER CANCEL: Set timeout to 10+ minutes
  ```
  Note: Release builds may fail due to GitVersion configuration requiring git repository metadata. Use Debug builds for development.

### Validation and Code Quality
- Always run formatting before committing:
  ```bash
  cd src
  dotnet format             # NEVER CANCEL: Takes ~10 seconds, fixes charset and formatting issues
  ```
- The repository has comprehensive .editorconfig rules and coding standards via Workleap.DotNet.CodingStandards package.
- CI will fail if code is not properly formatted.

## Validation
- Build and test successfully complete in Debug mode within seconds.
- ALWAYS run through at least one complete build and test cycle after making changes.
- ALWAYS run `dotnet format` before committing - the CI (.github/workflows/ci.yml) will fail without proper formatting.
- You can build and test the library, but this is a library project with no runnable application.
- **MANUAL VALIDATION**: The library API has been validated to compile and instantiate correctly. Key classes can be created and extension methods are available.

## Project Structure
- **Main library**: `src/Workleap.Extensions.Configuration.Secrets/` - Targets .NET 6.0 and .NET Standard 2.0
- **Test project**: `src/Workleap.Extensions.Configuration.Secrets.Tests/` - Targets .NET 8.0 (and .NET 4.7.2 on Windows)
- **Key classes**:
  - `TokenCredentialProvider` - Provides Azure credentials based on environment
  - `SecretClientProvider` - Provides Azure Key Vault SecretClient instances
  - `ServiceCollectionExtensions` - DI container extensions
- **Test framework**: xUnit with 2 existing tests

## Common Tasks

### Repository Root Structure
```
/home/runner/work/wl-extensions-configuration-secrets/wl-extensions-configuration-secrets/
├── Build.ps1                    # Main build script (Release mode)
├── README.md                    # Package documentation
├── global.json                  # .NET SDK version requirement (9.0.304)
├── src/
│   ├── Workleap.Extensions.Configuration.Secrets.sln
│   ├── Directory.Build.props    # Common MSBuild properties
│   ├── Workleap.Extensions.Configuration.Secrets/
│   └── Workleap.Extensions.Configuration.Secrets.Tests/
└── .github/
    └── workflows/ci.yml         # CI pipeline
```

### Key Development Files
- `src/Workleap.Extensions.Configuration.Secrets/ServiceCollectionExtensions.cs` - Main API surface
- `src/Workleap.Extensions.Configuration.Secrets/TokenCredentialProvider.cs` - Azure credential logic
- `src/Workleap.Extensions.Configuration.Secrets/SecretClientProvider.cs` - Key Vault client provider
- `src/Workleap.Extensions.Configuration.Secrets/PublicAPI.Shipped.txt` - API compatibility tracking


### Working with Release Builds
- Release builds via `Build.ps1` may fail due to GitVersion requirements (tested: fails with GitVersion MSBuild target errors)
- For development, prefer Debug builds: `dotnet build -c Debug`
- Release builds need proper git repository state and tags for versioning
- **NOTE**: Release builds currently fail in this environment due to GitVersion configuration, but Debug builds work reliably

### Package Information
- **Package ID**: Workleap.Extensions.Configuration.Secrets
- **NuGet Package**: Available on NuGet.org
- **License**: Apache-2.0
- **Assembly Signing**: Uses strong naming with Workleap.Extensions.Configuration.Secrets.snk