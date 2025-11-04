# Satellite Projects Build System

This directory contains the NUKE build system adapted for satellite projects in the lunar-snake-hub ecosystem. It provides a comprehensive build pipeline for creating NuGet packages from satellite projects.

## Overview

The build system is designed to:
- Build and test satellite projects
- Create NuGet packages with proper versioning
- Support local NuGet feed synchronization
- Provide flexible configuration via JSON
- Support both solution-based and individual project builds

## Quick Start

1. **Install Dependencies**
   ```bash
   dotnet tool restore
   ```

2. **Configure Build** (optional)
   ```bash
   cp build.config.json.example build.config.json
   # Edit build.config.json as needed
   ```

3. **Run Build**
   ```bash
   ./build.sh Compile    # macOS/Linux
   ./build.cmd Compile   # Windows
   ```

## Available Targets

### Core Targets
- **`Compile`** - Build all projects
- **`Clean`** - Clean build artifacts
- **`Restore`** - Restore NuGet packages
- **`Test`** - Run all tests
- **`Pack`** - Create NuGet packages
- **`Publish`** - Publish applications
- **`Release`** - Complete release pipeline

### Quality Targets
- **`Format`** - Format code using dotnet-format
- **`FormatCheck`** - Verify code formatting (CI-friendly)
- **`Analyze`** - Run analyzer fixes and build
- **`TestWithCoverage`** - Run tests with coverage collection

### Utility Targets
- **`PrintVersion`** - Display current version and artifact path
- **`BuildWebsite`** - Copy website artifacts (if present)
- **`SyncNugetLocal`** - Sync packages to local NuGet feed

## Configuration

The build system uses `build.config.json` for configuration. Create it from the example:

```json
{
  "sourceDir": "src",
  "websiteDir": "website",
  "frameworkDirs": ["src"],
  "pluginDirs": ["src"],
  "packPlugins": true,
  "packFramework": true,
  "excludePluginNames": [],
  "includePluginNames": [],
  "syncLocalNugetFeed": false,
  "localNugetFeedRoot": null,
  "localNugetFeedFlatSubdir": "flat",
  "localNugetFeedHierarchicalSubdir": "hierarchical",
  "localNugetFeedBaseUrl": null
}
```

### Configuration Options

- **`sourceDir`** - Source code directory (default: "src")
- **`websiteDir`** - Website directory (default: "website")
- **`frameworkDirs`** - Directories containing framework projects
- **`pluginDirs`** - Directories containing plugin projects
- **`packPlugins`** - Whether to pack plugin projects
- **`packFramework`** - Whether to pack framework projects
- **`excludePluginNames`** - Project names to exclude from packing
- **`includePluginNames`** - Project names to include in packing (exclusive)
- **`syncLocalNugetFeed`** - Enable local NuGet feed sync
- **`localNugetFeedRoot`** - Root directory for local feed

## Directory Structure

```
lunar-snake-hub/
├── build/
│   └── nuke/
│       ├── Build.cs                 # Main build script
│       ├── Build.csproj            # Build project file
│       ├── build.config.json        # Configuration (create from example)
│       ├── build.config.json.example
│       ├── build.cmd               # Windows build script
│       ├── build.ps1               # PowerShell build script
│       ├── build.sh                # Unix build script
│       └── Components/
│           └── IBuildConfig.cs     # Configuration interface
├── src/                          # Satellite projects source
│   ├── Project1/
│   │   └── Project1.csproj
│   └── Project2/
│       └── Project2.csproj
└── build/_artifacts/              # Build outputs
    └── {version}/
        ├── nuget/                 # NuGet packages
        ├── publish/                # Published applications
        ├── test-results/           # Test results
        └── test-reports/           # Test reports
```

## Versioning

The build system uses GitVersion for automatic versioning:
- Local builds: Default to "0.1.0-dev"
- CI builds: Use GitVersion SemVer
- Version is embedded in all packages and artifacts

## NuGet Package Creation

The `Pack` target creates NuGet packages with:
- Automatic versioning from GitVersion
- Symbol packages (.snupkg)
- Repository metadata (branch, commit)
- Output to versioned artifacts directory

### Local NuGet Feed Sync

Enable local feed sync in configuration:
```json
{
  "syncLocalNugetFeed": true,
  "localNugetFeedRoot": "/path/to/nuget-repo"
}
```

This creates both flat and hierarchical layouts:
- **Flat**: All packages in single directory
- **Hierarchical**: Organized by package ID/version
- **SHA512**: Hash files for integrity
- **Indexes**: JSON indexes for package discovery

## Usage Examples

### Basic Build
```bash
./build.sh Compile
```

### Full Release
```bash
./build.sh Release
```

### Package Only
```bash
./build.sh Pack
```

### Test with Coverage
```bash
./build.sh TestWithCoverage
```

### Format Code
```bash
./build.sh Format
```

### Check Formatting (CI)
```bash
./build.sh FormatCheck
```

## Integration with Satellite Projects

Satellite projects should:

1. **Structure projects** under `src/` directory
2. **Use standard .NET project files** (.csproj)
3. **Follow naming conventions** (avoid .Tests suffix for main projects)
4. **Configure package metadata** in project files:
   ```xml
   <PropertyGroup>
     <PackageId>YourProject</PackageId>
     <PackageVersion>1.0.0</PackageVersion>
     <Description>Your package description</Description>
     <Authors>Your Name</Authors>
   </PropertyGroup>
   ```

## CI/CD Integration

The build system is CI-friendly:
- **No interactive prompts**
- **Deterministic outputs**
- **Version-aware artifact paths**
- **Format checking for code quality**
- **Test result collection**

### Example CI Pipeline
```yaml
- name: Build and Test
  run: ./build.sh TestWithCoverage
  
- name: Pack
  run: ./build.sh Pack
  
- name: Upload Artifacts
  uses: actions/upload-artifact@v3
  with:
    path: build/_artifacts/
```

## Troubleshooting

### Common Issues

1. **Build fails to find solution**
   - Ensure `.sln` file exists in `src/` directory
   - Or rely on individual project discovery

2. **No packages created**
   - Check `frameworkDirs` and `pluginDirs` configuration
   - Verify projects have `GeneratePackageOnBuild` or run `Pack` target

3. **Version not set**
   - Ensure GitVersion is available
   - Check git repository has proper tags/commits

4. **Local feed sync fails**
   - Verify `localNugetFeedRoot` is writable
   - Check disk space and permissions

### Debug Mode

Enable verbose logging:
```bash
./build.sh --verbosity Verbose Compile
```

## Dependencies

- **.NET 8.0 SDK**
- **NUKE 8.0.0**
- **GitVersion.Tool 6.0.0**
- **Serilog** (for structured logging)

## Support

For issues and questions:
1. Check this README
2. Review `build.config.json.example`
3. Enable verbose logging
4. Examine build artifacts in `build/_artifacts/`
