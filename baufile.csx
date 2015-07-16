// parameters
var versionSuffix = Environment.GetEnvironmentVariable("VERSION_SUFFIX") ?? "-adhoc";
var msBuildFileVerbosity = (Verbosity)Enum.Parse(typeof(Verbosity), Environment.GetEnvironmentVariable("MSBUILD_FILE_VERBOSITY") ?? "minimal", true);
var nugetVerbosity = Environment.GetEnvironmentVariable("NUGET_VERBOSITY") ?? "quiet";

// solution specific variables
var version = File.ReadAllText("src/CommonAssemblyInfo.cs").Split(new[] { "AssemblyInformationalVersion(\"" }, 2, StringSplitOptions.None).ElementAt(1).Split(new[] { '"' }).First();
var nugetCommand = "scriptcs_packages/NuGet.CommandLine.2.8.2/tools/NuGet.exe";
var xunitCommand = "scriptcs_packages/xunit.runners.1.9.2/tools/xunit.console.clr4.exe";
var solution = "src/Bau.MSBuild.sln";
var output = "artifacts/output";
var tests = "artifacts/tests";
var logs = "artifacts/logs";
var unit = "src/test/Bau.MSBuild.Test.Unit/bin/Release/Bau.MSBuild.Test.Unit.dll";
var pack = "src/Bau.MSBuild/Bau.MSBuild";

// solution agnostic tasks
var bau = Require<Bau>();

bau
.Task("default").DependsOn("unit", "pack")

.Task("logs").Do(() => CreateDirectory(logs))

.MSBuild("clean").DependsOn("logs").Do(msb =>
{
    msb.MSBuildVersion = "net45";
    msb.Solution = solution;
    msb.Targets = new[] { "Clean", };
    msb.Properties = new { Configuration = "Release" };
    msb.MaxCpuCount = -1;
    msb.NodeReuse = false;
    msb.Verbosity = Verbosity.Minimal;
    msb.NoLogo = true;
    msb.FileLoggers.Add(new FileLogger
    {
        FileLoggerParameters = new FileLoggerParameters
        {
            PerformanceSummary = true,
            Summary = true,
            Verbosity = msBuildFileVerbosity,
            LogFile = logs + "/clean.log",
        }
    });
})

.Task("clobber").DependsOn("clean").Do(() => DeleteDirectory(output))

.Exec("restore").Do(exec => exec
    .Run(nugetCommand)
    .With("restore", solution))

.MSBuild("build").DependsOn("clean", "restore", "logs").Do(msb =>
{
    msb.MSBuildVersion = "net45";
    msb.Solution = solution;
    msb.Targets = new[] { "Build", };
    msb.Properties = new { Configuration = "Release" };
    msb.MaxCpuCount = -1;
    msb.NodeReuse = false;
    msb.Verbosity = Verbosity.Minimal;
    msb.NoLogo = true;
    msb.FileLoggers.Add(new FileLogger
    {
        FileLoggerParameters = new FileLoggerParameters
        {
            PerformanceSummary = true,
            Summary = true,
            Verbosity = msBuildFileVerbosity,
            LogFile = logs + "/build.log",
        }
    });
})

.Task("tests").Do(() => CreateDirectory(tests))

.Xunit("unit").DependsOn("build", "tests").Do(xunit => xunit
    .Use(xunitCommand).Run(unit).Html().Xml())

.Task("output").Do(() => CreateDirectory(output))

.Exec("pack").DependsOn("build", "clobber", "output").Do(exec => exec
    .Run(nugetCommand)
    .With(
        "pack", pack + ".csproj",
        "-OutputDirectory", output,
        "-Properties", "Configuration=Release",
        "-IncludeReferencedProjects",
        "-Verbosity", nugetVerbosity,
        "-Version", version + versionSuffix))

.Run();

void CreateDirectory(string name)
{
    if (!Directory.Exists(name))
    {
        Directory.CreateDirectory(name);
        System.Threading.Thread.Sleep(100); // HACK (adamralph): wait for the directory to be created
    }
}

void DeleteDirectory(string name)
{
    if (Directory.Exists(name))
    {
        Directory.Delete(name, true);
    }
}
