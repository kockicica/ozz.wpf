using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Nuke.Common;
using Nuke.Common.ChangeLog;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild {

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion]    readonly GitVersion    GitVersion;

    [Solution] readonly Solution Solution;

    [Parameter("Archives folder")] string ArchivesFolder = RootDirectory / "archives";

    [Parameter("List of modules to buld", List = true)] string[] Modules = { "ozz.wpf" };

    [Parameter("Output folder")] string OutputFolder = RootDirectory / "artifacts";

    [Parameter("List of runtimes", List = true)] string[] Runtimes = { "win-x64" };

    [Parameter("Self-contained")] bool SelfContained = true;

    AbsolutePath SourceDirectory => RootDirectory / "src";

    Target Clean => _ => _
                         .Before(Restore)
                         .Executes(() =>
                         {
                             foreach (var project in GetProjectsToBuild()) {
                                 project
                                     .Directory
                                     .GlobDirectories("**/bin", "**/obj")
                                     .Select(x => (string)x)
                                     .ForEach(DeleteDirectory);
                             }
                             EnsureCleanDirectory(OutputFolder);
                         });

    Target CleanArtifacts => _ => _
        .Executes(() =>
        {
            EnsureCleanDirectory(ArchivesFolder);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                              .SetProjectFile(Solution));
        });

    Target Compile => _ => _
                           .DependsOn(Restore)
                           .Executes(() =>
                           {
                               DotNetBuild(s => s
                                                .SetProjectFile(Solution)
                                                .SetConfiguration(Configuration)
                                                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                                                .SetFileVersion(GitVersion.AssemblySemFileVer)
                                                .SetInformationalVersion(GitVersion.InformationalVersion)
                                                .EnableNoRestore());
                           });

    Target Publish => _ => _
                           .DependsOn(Clean, CleanArtifacts)
                           .Executes(() =>
                           {
                               Logger.Normal(GetProjectsToBuild().Select(m => m.Name).JoinComma());
                               foreach (var project in GetProjectsToBuild()) {
                                   foreach (var runtime in Runtimes) {
                                       var outputFolder = Path.Combine(OutputFolder, runtime);
                                       PublishModule(project, outputFolder, runtime);
                                   }
                               }

                           });

    Target CreateArchives => _ => _
                                  .DependsOn(Publish)
                                  .Executes(() =>
                                  {
                                      foreach (var project in GetProjectsToBuild()) {
                                          foreach (var runtime in Runtimes) {
                                              var items = Path.Combine(OutputFolder, runtime);
                                              var fileName = SelfContained
                                                  ? $"{project.Name}-{GitVersion.SemVer}-{runtime}.zip"
                                                  : $"{project.Name}-{GitVersion.SemVer}-{runtime}-fxdependent.zip";
                                              var archiveFile = Path.Combine(ArchivesFolder, fileName);
                                              DeleteFile(archiveFile);
                                              //CompressionTasks.CompressZip(items, archiveFile, info => info.Directory.Name.StartsWith(project.Name));
                                              CompressionTasks.CompressZip(items, archiveFile, compressionLevel: CompressionLevel.SmallestSize);
                                          }
                                      }

                                  });

    Target Changelog => _ => _
        .Executes(() =>
        {
            ChangelogTasks.FinalizeChangelog(RootDirectory / "CHANGELOG.md", GitVersion.SemVer, GitRepository);
        });

    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);


    Project GetProject(string projectName) {
        return Solution.AllProjects.SingleOrDefault(pr => pr.Name == projectName);
    }

    IEnumerable<Project> GetProjectsToBuild() {
        return Solution.AllProjects.Where(p => Modules.Contains(p.Name));
    }

    IReadOnlyCollection<Output> PublishModule(Project project, string outDir, string runtime) {
        if (project != null) {
            var outputFolder = Path.Combine(outDir, project.Name);
            return DotNetPublish(settings => settings
                                             .SetProject(project)
                                             .SetOutput(outputFolder)
                                             .SetRuntime(runtime)
                                             .SetConfiguration(Configuration)
                                             .SetSelfContained(SelfContained)
                                             .EnablePublishSingleFile()
                                             .EnablePublishTrimmed()
                                             .SetVersion(GitVersion.SemVer)
                                             .SetAssemblyVersion(GitVersion.AssemblySemVer)
                                             .SetFileVersion(GitVersion.AssemblySemFileVer)
            );
        }

        return null;
    }
}