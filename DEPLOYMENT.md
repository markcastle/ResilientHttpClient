# Deployment Guide

## Prerequisites

Before deploying to NuGet, ensure you have:

1. **NuGet Account**: Create an account at [nuget.org](https://www.nuget.org/)
2. **API Key**: Generate an API key from your NuGet account settings
3. **GitHub Repository**: Project is pushed to GitHub

---

## GitHub Actions Setup

### Required Secrets

Add the following secret to your GitHub repository:

1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret**
3. Add:
   - **Name**: `NUGET_API_KEY`
   - **Value**: Your NuGet API key from nuget.org

### CI/CD Pipeline

The GitHub Actions workflow (`.github/workflows/ci.yml`) automatically:

- ✅ Runs on push/PR to master/main/develop branches
- ✅ Builds and tests the solution
- ✅ Generates and uploads coverage reports
- ✅ Enforces 80% coverage threshold
- ✅ Publishes to NuGet on tagged releases

---

## Manual Deployment to NuGet

If you prefer manual deployment:

```bash
# 1. Build the project
dotnet build --configuration Release

# 2. Pack the NuGet package
dotnet pack ResilientHttpClient.Core/ResilientHttpClient.Core.csproj --configuration Release --output ./artifacts

# 3. Push to NuGet
dotnet nuget push ./artifacts/*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

---

## Versioning

Update the version in `ResilientHttpClient.Core/ResilientHttpClient.Core.csproj`:

```xml
<PackageVersion>1.0.0</PackageVersion>
```

Follow [Semantic Versioning](https://semver.org/):
- **Major**: Breaking changes (e.g., 2.0.0)
- **Minor**: New features, backward compatible (e.g., 1.1.0)
- **Patch**: Bug fixes (e.g., 1.0.1)

---

## Release Process

1. **Update Version**: Increment version in `.csproj`
2. **Update CHANGELOG**: Document changes
3. **Commit Changes**: `git commit -am "Release v1.0.1"`
4. **Create Tag**: `git tag v1.0.1`
5. **Push Tag**: `git push origin v1.0.1`
6. **GitHub Actions**: Automatically builds and publishes to NuGet

---

## Manual Testing Before Release

```bash
# Run all tests
dotnet test

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coveragereport

# Build NuGet package locally
dotnet pack --configuration Release --output ./artifacts

# Test the package locally
dotnet add package ResilientHttpClient.Core --source ./artifacts
```

---

## Post-Deployment

1. Verify package is live at: `https://www.nuget.org/packages/ResilientHttpClient.Core/`
2. Test installation: `dotnet add package ResilientHttpClient.Core`
3. Update README badges if needed
4. Announce release on relevant channels

---

## Troubleshooting

### Package Already Exists Error
NuGet doesn't allow overwriting versions. Increment the version number and try again.

### Coverage Below Threshold
The CI pipeline fails if coverage drops below 80%. Add tests to increase coverage before merging.

### GitHub Actions Not Running
Check that the workflow file is in `.github/workflows/` and the repository has Actions enabled in Settings.

---

## Local Development Workflow

```bash
# 1. Make changes
# 2. Run tests
dotnet test

# 3. Check coverage
.\coverage.bat

# 4. Commit and push
git add .
git commit -m "Your changes"
git push origin develop
```

---

## Support

For issues or questions:
- Open an issue on GitHub
- Check existing documentation
- Review test files for usage examples
