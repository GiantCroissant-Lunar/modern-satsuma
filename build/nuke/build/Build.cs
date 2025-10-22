using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tooling;
using Serilog;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Pack);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    AbsolutePath SourceDirectory => RootDirectory / ".." / ".." / "dotnet";
    AbsolutePath ArtifactsDirectory => RootDirectory / ".." / "_artifacts";
    
    string Version => GetGitVersion();
    AbsolutePath PackageDirectory => ArtifactsDirectory / Version;

    string GetGitVersion()
    {
        try
        {
            var outputs = ProcessTasks.StartProcess("dotnet", "dotnet-gitversion /showvariable SemVer", RootDirectory / ".." / "..")
                .AssertZeroExitCode()
                .Output;
            
            foreach (var output in outputs)
            {
                if (!string.IsNullOrEmpty(output.Text))
                {
                    return output.Text.Trim();
                }
            }
        }
        catch (System.Exception ex)
        {
            Log.Warning("GitVersion failed: {Message}", ex.Message);
        }
        
        return "1.0.0-alpha.1";
    }

    string GetGitVersionVariable(string variable)
    {
        try
        {
            var outputs = ProcessTasks.StartProcess("dotnet", $"dotnet-gitversion /showvariable {variable}", RootDirectory / ".." / "..")
                .AssertZeroExitCode()
                .Output;
            
            foreach (var output in outputs)
            {
                if (!string.IsNullOrEmpty(output.Text))
                {
                    return output.Text.Trim();
                }
            }
        }
        catch
        {
            // Ignore errors and return empty string
        }
        
        return "";
    }

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            var binObjDirs = (SourceDirectory / "framework").GlobDirectories("**/bin", "**/obj");
            foreach (var dir in binObjDirs)
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
            
            if (Directory.Exists(ArtifactsDirectory))
                Directory.Delete(ArtifactsDirectory, true);
            Directory.CreateDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(SourceDirectory / "framework" / "Plate.ModernSatsuma.sln"));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            var assemblyVersion = GetGitVersionVariable("AssemblySemVer");
            var fileVersion = GetGitVersionVariable("AssemblySemFileVer");
            var informationalVersion = GetGitVersionVariable("InformationalVersion");

            DotNetBuild(s => s
                .SetProjectFile(SourceDirectory / "framework" / "Plate.ModernSatsuma.sln")
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(assemblyVersion)
                .SetFileVersion(fileVersion)
                .SetInformationalVersion(informationalVersion)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(SourceDirectory / "framework" / "Plate.ModernSatsuma.sln")
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore());
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Produces(PackageDirectory / "*.nupkg")
        .Executes(() =>
        {
            Directory.CreateDirectory(PackageDirectory);
            
            var nugetVersion = GetGitVersionVariable("SemVer");
            var assemblyVersion = GetGitVersionVariable("AssemblySemVer");
            var fileVersion = GetGitVersionVariable("AssemblySemFileVer");
            var informationalVersion = GetGitVersionVariable("InformationalVersion");
            
            DotNetPack(s => s
                .SetProject(SourceDirectory / "framework" / "src" / "Plate.ModernSatsuma" / "Plate.ModernSatsuma.csproj")
                .SetConfiguration(Configuration)
                .SetOutputDirectory(PackageDirectory)
                .SetVersion(nugetVersion)
                .SetAssemblyVersion(assemblyVersion)
                .SetFileVersion(fileVersion)
                .SetInformationalVersion(informationalVersion)
                .EnableNoBuild()
                .EnableNoRestore());

            Log.Information("Package created: {Version}", nugetVersion);
            Log.Information("Output directory: {Directory}", PackageDirectory);
        });

    Target Publish => _ => _
        .DependsOn(Pack)
        .Executes(() =>
        {
            Log.Information("Packages available at: {Directory}", PackageDirectory);
            var packages = Directory.GetFiles(PackageDirectory, "*.nupkg");
            foreach (var package in packages)
            {
                Log.Information("  - {Package}", Path.GetFileName(package));
            }
        });
}
