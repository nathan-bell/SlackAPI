#addin nuget:?package=Cake.Git
#addin "MagicChunks"

var configuration = Argument("configuration", "Release");
var target = Argument("target", "Default");

var project = File("./SlackAPI/project.json");
var testProject = File("./SlackAPI.Tests/project.json");
var projects = new[] { project, testProject };
var artifactsDirectory = Directory("./artifacts");
var revision = AppVeyor.IsRunningOnAppVeyor ? AppVeyor.Environment.Build.Number : 0;
var version = AppVeyor.IsRunningOnAppVeyor ? new Version(AppVeyor.Environment.Build.Version).ToString(3) : "1.0.0";
var globalAssemblyInfo = File("./GlobalAssemblyVersion.cs");

var generatedVersion = "";
var generatedSuffix = "";


Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDirectory);
});


Task("Restore-Packages")
    .Does(() =>
{
    foreach(var project in projects)
    {
        DotNetCoreRestore(project);
    }
});


Task("Generate-Versionning")
    .Does(() =>
{
    generatedVersion = version + "." + revision;
    Information("Generated version '{0}'", generatedVersion);

    var branch = (AppVeyor.IsRunningOnAppVeyor ? AppVeyor.Environment.Repository.Branch : GitBranchCurrent(".").FriendlyName).Replace('/', '-');
    generatedSuffix = (branch == "master" && revision > 0) ? "" : branch.Substring(0, Math.Min(10, branch.Length)) + "-" + revision;
    Information("Generated suffix '{0}'", generatedSuffix);
});


Task("Patch-GlobalAssemblyVersions")
    .IsDependentOn("Generate-Versionning")
    .Does(() =>
{
    CreateAssemblyInfo(globalAssemblyInfo, new AssemblyInfoSettings {
        FileVersion = generatedVersion,
        InformationalVersion = version + "-" + generatedSuffix,
        Version = generatedVersion
        }
    );
});


Task("Patch-ProjectJson")
    .IsDependentOn("Generate-Versionning")
    .Does(() =>
{
    foreach(var project in projects)
    {
        TransformConfig(
            project,
            project,
            new TransformationCollection
            {
                { "version", version + "-*" }
            }
        );
    }
});


Task("Build")
    .IsDependentOn("Restore-Packages")
    .IsDependentOn("Patch-GlobalAssemblyVersions")
    .IsDependentOn("Patch-ProjectJson")
    .Does(() =>
{
    foreach(var project in projects)
    {
        DotNetCoreBuild(
            project,
            new DotNetCoreBuildSettings
            {
                Configuration = configuration
            }
        );
    }
});


Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest(
        testProject,
        new DotNetCoreTestSettings
        {
            Configuration = configuration
        }
    );
});


Task("Pack")
    .IsDependentOn("Clean")
    .IsDependentOn("Test")
    .Does(() =>
{
    DotNetCorePack(
        project,
        new DotNetCorePackSettings
        {
            Configuration = configuration,
            OutputDirectory = artifactsDirectory,
            VersionSuffix = generatedSuffix
        }
    );

});


Task("Default")
    .IsDependentOn("Pack");


RunTarget(target);