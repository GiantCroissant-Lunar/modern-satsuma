using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.PathConstruction;
using System.IO.Compression;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

// Simple BuildConfig class
public class BuildConfig
{
    public string SourceDir { get; set; } = "src";
    public string WebsiteDir { get; set; } = "website";
    public List<string> FrameworkDirs { get; set; } = new List<string> { "src" };
    public List<string> IncludePluginNames { get; set; } = new List<string>();
    public List<string> ExcludePluginNames { get; set; } = new List<string>();

    public static BuildConfig Load(AbsolutePath configPath)
    {
        if (!System.IO.File.Exists(configPath))
        {
            return new BuildConfig();
        }

        try
        {
            var content = System.IO.File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<BuildConfig>(content, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            }) ?? new BuildConfig();
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Failed to load build config from {Path}, using defaults", configPath);
            return new BuildConfig();
        }
    }
}

[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Pack);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

    [Solution]
    readonly Solution Solution;

    [GitVersion(Framework = "net8.0", NoFetch = true)]
    readonly GitVersion GitVersion;

    string Version => GitVersion?.SemVer ?? "0.1.0-dev";

    // Provide concrete accessors for configuration to avoid relying on default interface implementations
    AbsolutePath BuildConfigPath => RootDirectory / "build" / "nuke" / "build.config.json";
    BuildConfig Config => BuildConfig.Load(BuildConfigPath);

    AbsolutePath SourceDirectory => RootDirectory / (Config?.SourceDir ?? "src");
    AbsolutePath WebsiteDirectory => RootDirectory / (Config?.WebsiteDir ?? "website");
    AbsolutePath SchemasDirectory => RootDirectory / "schemas";
    AbsolutePath GeneratedDirectory => SourceDirectory / "generated";
    AbsolutePath BuildArtifactsDirectory => RootDirectory / "build" / "_artifacts";
    AbsolutePath VersionedArtifactsDirectory => BuildArtifactsDirectory / Version;
    AbsolutePath PublishDirectory => VersionedArtifactsDirectory / "publish";
    AbsolutePath NugetDirectory => VersionedArtifactsDirectory / "nuget";
    AbsolutePath LogsDirectory => VersionedArtifactsDirectory / "logs";
    AbsolutePath TestResultsDirectory => VersionedArtifactsDirectory / "test-results";
    AbsolutePath TestReportsDirectory => VersionedArtifactsDirectory / "test-reports";
    AbsolutePath SessionReportsDirectory => VersionedArtifactsDirectory / "reports" / "sessions";

    Target PrintVersion => _ => _
        .Executes(() =>
        {
            Serilog.Log.Information("Version: {Version}", Version);
            if (GitVersion != null)
            {
                Serilog.Log.Information("Branch: {Branch}", GitVersion.BranchName);
                Serilog.Log.Information("Commit: {Commit}", GitVersion.Sha);
            }
            Serilog.Log.Information("Artifacts: {Path}", VersionedArtifactsDirectory);
        });

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(d =>
            {
                if (System.IO.Directory.Exists(d))
                    DeleteDirectory(d);
            });

            // Skip website cleaning if directory doesn't exist or is incomplete
            if (System.IO.Directory.Exists(WebsiteDirectory))
            {
                try
                {
                    WebsiteDirectory.GlobDirectories("**/dist").ForEach(d =>
                    {
                        if (System.IO.Directory.Exists(d))
                            DeleteDirectory(d);
                    });
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    Serilog.Log.Warning("Skipping website cleanup due to missing directories");
                }
            }

            BuildArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            if (System.IO.File.Exists(SourceDirectory / "*.sln"))
            {
                DotNetRestore(s => s
                    .SetProjectFile(SourceDirectory / "*.sln"));
            }
            else
            {
                // Fallback to individual projects
                var projects = SourceDirectory.GlobFiles("**/*.csproj");
                foreach (var project in projects)
                {
                    DotNetRestore(s => s
                        .SetProjectFile(project));
                }
            }
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            if (Solution != null)
            {
                DotNetBuild(s => s
                    .SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore());
            }
            else
            {
                // Fallback to individual projects
                var projects = SourceDirectory.GlobFiles("**/*.csproj")
                    .Where(p => !p.ToString().EndsWith(".Tests.csproj", StringComparison.OrdinalIgnoreCase));
                
                foreach (var project in projects)
                {
                    Serilog.Log.Information("Building {Project}...", project);
                    DotNetBuild(s => s
                        .SetProjectFile(project)
                        .SetConfiguration(Configuration)
                        .EnableNoRestore());
                }
            }
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var testProjects = SourceDirectory.GlobFiles("**/*.Tests.csproj");
            
            foreach (var testProject in testProjects)
            {
                Serilog.Log.Information("Running tests for {Project}...", testProject);
                DotNetTest(s => s
                    .SetProjectFile(testProject)
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore());
            }
        });

    // Formats code using dotnet-format and optionally JetBrains ReSharper Global Tools (jb)
    Target Format => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            // dotnet-format (built into .NET SDK): applies whitespace, style, and analyzer code fixes when available
            Serilog.Log.Information("Running dotnet format (whitespace, style, analyzers)...");

            if (Solution != null)
            {
                DotNet($"format \"{Solution}\" --verbosity minimal", workingDirectory: RootDirectory);
            }
            else
            {
                var projects = SourceDirectory.GlobFiles("**/*.csproj");
                foreach (var project in projects)
                {
                    DotNet($"format \"{project}\" --verbosity minimal", workingDirectory: RootDirectory);
                }
            }

            // ReSharper Global Tools (optional): enforces EditorConfig wrapping like one-parameter-per-line
            // Tries default installation path then PATH
            try
            {
                var jbDefault = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".dotnet", "tools", "jb.exe");

                bool ranReSharper = false;
                if (System.IO.File.Exists(jbDefault))
                {
                    Serilog.Log.Information("Running ReSharper cleanupcode via {Path}...", jbDefault);
                    var solutionPath = Solution?.Path ?? (SourceDirectory / "*.sln");
                    var p1 = ProcessTasks.StartProcess(jbDefault, $"cleanupcode \"{solutionPath}\"", RootDirectory);
                    p1.AssertZeroExitCode();
                    ranReSharper = true;
                }
                if (!ranReSharper)
                {
                    try
                    {
                        Serilog.Log.Information("Trying 'jb' from PATH for ReSharper cleanupcode...");
                        var solutionPath = Solution?.Path ?? (SourceDirectory / "*.sln");
                        var p2 = ProcessTasks.StartProcess("jb", $"cleanupcode \"{solutionPath}\"", RootDirectory);
                        p2.AssertZeroExitCode();
                        ranReSharper = true;
                    }
                    catch { /* ignore if not installed */ }
                }

                if (!ranReSharper)
                {
                    Serilog.Log.Information("ReSharper Global Tools (jb) not found. To enable ReSharper-based formatting: dotnet tool install -g JetBrains.ReSharper.GlobalTools");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning("ReSharper cleanup skipped: {Message}", ex.Message);
            }
        });

    // Verifies formatting and analyzer diagnostics without changing files (CI-friendly)
    Target FormatCheck => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            Serilog.Log.Information("Verifying formatting (no changes)...");

            if (Solution != null)
            {
                DotNet($"format \"{Solution}\" --verify-no-changes --verbosity minimal", workingDirectory: RootDirectory);
                DotNet($"format analyzers \"{Solution}\" --verify-no-changes --verbosity minimal", workingDirectory: RootDirectory);
            }
            else
            {
                var projects = SourceDirectory.GlobFiles("**/*.csproj");
                foreach (var project in projects)
                {
                    DotNet($"format \"{project}\" --verify-no-changes --verbosity minimal", workingDirectory: RootDirectory);
                    DotNet($"format analyzers \"{project}\" --verify-no-changes --verbosity minimal", workingDirectory: RootDirectory);
                }
            }
        });

    // Runs analyzer fixes (where possible) and builds to enforce analyzer severities
    Target Analyze => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            Serilog.Log.Information("Running analyzer fixes where available (dotnet format analyzers)...");

            if (Solution != null)
            {
                DotNet($"format analyzers \"{Solution}\" --verbosity minimal", workingDirectory: RootDirectory);
                DotNetBuild(s => s
                    .SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore());
            }
            else
            {
                var projects = SourceDirectory.GlobFiles("**/*.csproj");
                foreach (var project in projects)
                {
                    DotNet($"format analyzers \"{project}\" --verbosity minimal", workingDirectory: RootDirectory);
                    DotNetBuild(s => s
                        .SetProjectFile(project)
                        .SetConfiguration(Configuration)
                        .EnableNoRestore());
                }
            }
        });

    Target TestWithCoverage => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            TestResultsDirectory.CreateOrCleanDirectory();

            var testProjects = SourceDirectory.GlobFiles("**/*.Tests.csproj");

            foreach (var testProject in testProjects)
            {
                var projectName = System.IO.Path.GetFileNameWithoutExtension(testProject);
                var testResultFile = TestResultsDirectory / $"{projectName}.trx";

                Serilog.Log.Information("Running tests for {Project}...", projectName);

                DotNetTest(s => s
                    .SetProjectFile(testProject)
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetLoggers($"trx;LogFileName={testResultFile}")
                    .SetDataCollector("XPlat Code Coverage")
                    .SetResultsDirectory(TestResultsDirectory));
            }

            Serilog.Log.Information("Test results saved to: {Path}", TestResultsDirectory);
        });

    // Builds NuGet packages for satellite projects into versioned artifacts
    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            NugetDirectory.CreateOrCleanDirectory();

            // Load satellite configuration from hub manifest
            var hubManifestPath = RootDirectory / ".hub-manifest.json";
            var hubManifest = LoadHubManifest(hubManifestPath);
            
            var satelliteRoots = (Config?.FrameworkDirs ?? new List<string> { "src" })
                .Select(d => RootDirectory / d);

            var satelliteProjects = satelliteRoots
                .SelectMany(r => r.GlobFiles("**/*.csproj"))
                .Where(p => !p.ToString().EndsWith(".Tests.csproj", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Apply satellite project filters from hub manifest
            if (hubManifest?.Satellites?.Build?.IncludeProjects != null && hubManifest.Satellites.Build.IncludeProjects.Count > 0)
            {
                satelliteProjects = satelliteProjects
                    .Where(p => hubManifest.Satellites.Build.IncludeProjects.Contains(System.IO.Path.GetFileNameWithoutExtension(p)))
                    .ToList();
            }
            if (hubManifest?.Satellites?.Build?.ExcludeProjects != null && hubManifest.Satellites.Build.ExcludeProjects.Count > 0)
            {
                satelliteProjects = satelliteProjects
                    .Where(p => !hubManifest.Satellites.Build.ExcludeProjects.Contains(System.IO.Path.GetFileNameWithoutExtension(p)))
                    .ToList();
            }

            // Also apply any config-based filters (fallback)
            if (Config?.IncludePluginNames != null && Config.IncludePluginNames.Count > 0)
            {
                satelliteProjects = satelliteProjects
                    .Where(p => Config.IncludePluginNames.Contains(System.IO.Path.GetFileNameWithoutExtension(p)))
                    .ToList();
            }
            if (Config?.ExcludePluginNames != null && Config.ExcludePluginNames.Count > 0)
            {
                satelliteProjects = satelliteProjects
                    .Where(p => !Config.ExcludePluginNames.Contains(System.IO.Path.GetFileNameWithoutExtension(p)))
                    .ToList();
            }

            if (satelliteProjects.Count == 0)
            {
                Serilog.Log.Warning("No satellite projects found to pack under: {Roots}", string.Join(", ", satelliteRoots));
                return;
            }

            void PackProject(AbsolutePath csproj)
            {
                Serilog.Log.Information("Packing {Project} -> {OutDir}", csproj, NugetDirectory);
                
                // Build first to ensure outputs exist
                DotNetBuild(s => s
                    .SetProjectFile(csproj)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore());

                // Pack with version from GitVersion
                DotNetPack(s => s
                    .SetProject(csproj)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .EnableNoBuild()
                    .SetOutputDirectory(NugetDirectory)
                    .SetVersion(Version)
                    .SetIncludeSymbols(true)
                    .SetSymbolPackageFormat(Nuke.Common.Tools.DotNet.DotNetSymbolPackageFormat.snupkg)
                    .SetProperty("RepositoryBranch", GitVersion?.BranchName ?? "local")
                    .SetProperty("RepositoryCommit", GitVersion?.Sha ?? "local"));

                // Also copy any packages generated by build
                try
                {
                    var binDir = csproj.Parent / "bin" / Configuration;
                    if (System.IO.Directory.Exists(binDir))
                    {
                        var pkgs = System.IO.Directory.GetFiles(binDir, "*.nupkg", System.IO.SearchOption.AllDirectories)
                            .Concat(System.IO.Directory.GetFiles(binDir, "*.snupkg", System.IO.SearchOption.AllDirectories))
                            .ToArray();
                        foreach (var pkg in pkgs)
                        {
                            var dest = NugetDirectory / System.IO.Path.GetFileName(pkg);
                            System.IO.File.Copy(pkg, dest, true);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Serilog.Log.Warning("Failed to copy build-generated packages for {Project}: {Message}", csproj, ex.Message);
                }
            }

            satelliteProjects.ForEach(PackProject);

            Serilog.Log.Information("✅ Satellite packages created at: {Path}", NugetDirectory);
        });

    Target Publish => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            if (Solution != null)
            {
                var projects = Solution.GetAllProjects("*.csproj")
                    .Where(p => !p.Name.EndsWith(".Tests"));
                
                foreach (var project in projects)
                {
                    var projectName = project.Name;
                    var outputDir = PublishDirectory / projectName;
                    
                    Serilog.Log.Information("Publishing {Project}...", projectName);
                    DotNetPublish(s => s
                        .SetProject(project)
                        .SetConfiguration(Configuration)
                        .SetOutput(outputDir)
                        .EnableNoRestore());
                }
            }
            else
            {
                var projects = SourceDirectory.GlobFiles("**/*.csproj")
                    .Where(p => !p.ToString().EndsWith(".Tests.csproj", StringComparison.OrdinalIgnoreCase));
                
                foreach (var project in projects)
                {
                    var projectName = System.IO.Path.GetFileNameWithoutExtension(project);
                    var outputDir = PublishDirectory / projectName;
                    
                    Serilog.Log.Information("Publishing {Project}...", projectName);
                    DotNetPublish(s => s
                        .SetProject(project)
                        .SetConfiguration(Configuration)
                        .SetOutput(outputDir)
                        .EnableNoRestore());
                }
            }
        });

    Target BuildWebsite => _ => _
        .Executes(() =>
        {
            Serilog.Log.Information("Copying website artifacts...");

            // Expect website to be already built by separate build process
            var webDist = WebsiteDirectory / "dist";
            if (!System.IO.Directory.Exists(webDist))
            {
                throw new Exception("Website not built. Please run website build process first");
            }

            // Copy built website to artifacts
            var websiteArtifacts = PublishDirectory / "website";
            websiteArtifacts.CreateOrCleanDirectory();

            CopyDirectoryRecursively(
                webDist,
                websiteArtifacts,
                DirectoryExistsPolicy.Merge);

            Serilog.Log.Information("Website artifacts copied to: {Path}", websiteArtifacts);
        });

    Target Release => _ => _
        .DependsOn(Clean, PrintVersion, Publish, Pack, BuildWebsite)
        .Executes(() =>
        {
            Serilog.Log.Information("Release artifacts created at: {Path}", VersionedArtifactsDirectory);
            Serilog.Log.Information("Version: {Version}", Version);

            // Create directory structure
            LogsDirectory.CreateDirectory();
            TestResultsDirectory.CreateDirectory();
            TestReportsDirectory.CreateDirectory();
            SessionReportsDirectory.CreateDirectory();

            // Create .gitkeep files
            System.IO.File.WriteAllText(LogsDirectory / ".gitkeep", "");
            System.IO.File.WriteAllText(TestResultsDirectory / ".gitkeep", "");
            System.IO.File.WriteAllText(TestReportsDirectory / ".gitkeep", "");
            System.IO.File.WriteAllText(SessionReportsDirectory / ".gitkeep", "");

            // Create version info file
            var versionFile = VersionedArtifactsDirectory / "version.json";
            System.IO.File.WriteAllText(versionFile, System.Text.Json.JsonSerializer.Serialize(new
            {
                version = Version,
                fullSemVer = GitVersion?.FullSemVer ?? Version,
                branch = GitVersion?.BranchName ?? "unknown",
                commit = GitVersion?.Sha ?? "unknown",
                buildDate = DateTime.UtcNow.ToString("o"),
                components = new string[] { "satellite", "nuget" },
                directories = new
                {
                    publish = "publish/",
                    nuget = "nuget/",
                    logs = "logs/",
                    testResults = "test-results/",
                    testReports = "test-reports/",
                    sessionReports = "reports/sessions/"
                }
            }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

            Serilog.Log.Information("Version file created: {File}", versionFile);

            // List all artifacts
            Serilog.Log.Information("\n=== Release Artifacts ===");
            Serilog.Log.Information("Version: {Version}", Version);
            Serilog.Log.Information("Publish: {Path}", PublishDirectory);
            Serilog.Log.Information("Website: {Path}", PublishDirectory / "website");
            Serilog.Log.Information("NuGet: {Path}", NugetDirectory);
            Serilog.Log.Information("Logs: {Path}", LogsDirectory);
            Serilog.Log.Information("Test Results: {Path}", TestResultsDirectory);
            Serilog.Log.Information("Test Reports: {Path}", TestReportsDirectory);
            Serilog.Log.Information("Session Reports: {Path}", SessionReportsDirectory);
            Serilog.Log.Information("========================\n");
        });

    // Hub manifest configuration classes
    // TODO: Generate these with quicktype from JSON schema for better type safety
    public class HubManifest
    {
        public HubConfig Hub { get; set; }
        public Dictionary<string, string> Packs { get; set; }
        public ManifestConfig Config { get; set; }
        public SatelliteConfig Satellites { get; set; }
        public SyncConfig Sync { get; set; }
        public MetadataConfig Metadata { get; set; }
    }

    public class HubConfig
    {
        public string Repo { get; set; }
        public string Branch { get; set; }
    }

    public class ManifestConfig
    {
        [JsonPropertyName("cache_dir")]
        public string CacheDir { get; set; }

        [JsonPropertyName("symlink_agents")]
        public bool SymlinkAgents { get; set; }
    }

    public class SatelliteConfig
    {
        public SatelliteBuildConfig Build { get; set; }
        public SatellitePackagingConfig Packaging { get; set; }
        public SatelliteSyncConfig Sync { get; set; }
    }

    public class SatelliteBuildConfig
    {
        [JsonPropertyName("include_projects")]
        public List<string> IncludeProjects { get; set; } = new List<string>();

        [JsonPropertyName("exclude_projects")]
        public List<string> ExcludeProjects { get; set; } = new List<string>();
    }

    public class SatellitePackagingConfig
    {
        [JsonPropertyName("local_feed_path")]
        public string LocalFeedPath { get; set; }

        [JsonPropertyName("sync_to_local_feed")]
        public bool SyncToLocalFeed { get; set; }

        [JsonPropertyName("artifact_base_dir")]
        public string ArtifactBaseDir { get; set; }
    }

    public class SatelliteSyncConfig
    {
        [JsonPropertyName("sync_nuke_build")]
        public bool SyncNukeBuild { get; set; }

        [JsonPropertyName("nuke_source_dir")]
        public string NukeSourceDir { get; set; }

        [JsonPropertyName("nuke_target_dir")]
        public string NukeTargetDir { get; set; }

        [JsonPropertyName("sync_patterns")]
        public List<string> SyncPatterns { get; set; } = new List<string>();
    }

    public class SyncConfig
    {
        public List<string> Include { get; set; } = new List<string>();
    }

    public class MetadataConfig
    {
        public string Version { get; set; }

        [JsonPropertyName("last_updated")]
        public string LastUpdated { get; set; }

        public string Description { get; set; }
    }

    static HubManifest LoadHubManifest(AbsolutePath manifestPath)
    {
        if (!System.IO.File.Exists(manifestPath))
        {
            Serilog.Log.Warning("Hub manifest not found at {Path}", manifestPath);
            return null;
        }

        try
        {
            var content = System.IO.File.ReadAllText(manifestPath);
            var manifest = JsonSerializer.Deserialize<HubManifest>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

            if (manifest == null)
            {
                Serilog.Log.Warning("Failed to parse hub manifest");
                return null;
            }

            if (manifest.Satellites == null)
            {
                Serilog.Log.Warning("No satellites section found in hub manifest");
                return null;
            }

            return manifest;
        }
        catch (System.Exception ex)
        {
            Serilog.Log.Error(ex, "Failed to load hub manifest from {Path}", manifestPath);
            return null;
        }
    }
}
