// parameters
var versionSuffix = Environment.GetEnvironmentVariable("VERSION_SUFFIX") ?? "-adhoc";
var msBuildFileVerbosity = (Verbosity)Enum.Parse(typeof(Verbosity), Environment.GetEnvironmentVariable("MSBUILD_FILE_VERBOSITY") ?? "normal", true);
var nugetVerbosity = Environment.GetEnvironmentVariable("NUGET_VERBOSITY") ?? "quiet";

// solution specific variables
var version = File.ReadAllText("src/CommonAssemblyInfo.cs").Split(new[] { "AssemblyInformationalVersion(\"" }, 2, StringSplitOptions.None).ElementAt(1).Split(new[] { '"' }).First();
var nugetCommand = "packages/NuGet.CommandLine.2.8.2/tools/NuGet.exe";
var xunitCommand = "packages/xunit.runners.1.9.2/tools/xunit.console.clr4.exe";
var solution = "src/Bau.MSBuild.sln";
var output = "artifacts/output";
var tests = "artifacts/tests";
var logs = "artifacts/logs";
var unit = "src/test/Bau.MSBuild.Test.Unit/bin/Release/Bau.MSBuild.Test.Unit.dll";
var packs = new[] { "src/Bau.MSBuild/Bau.MSBuild", };

// solution agnostic tasks
Require<Bau>()

.Task("default").DependsOn("unit", "pack")

.Task("logs").Do(() =>
{
    if (!Directory.Exists(logs))
    {
        Directory.CreateDirectory(logs);
        System.Threading.Thread.Sleep(100); // HACK (adamralph): wait for the directory to be created
    }
})

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

.Task("clobber").DependsOn("clean").Do(() =>
{
    if (Directory.Exists(output))
    {
        Directory.Delete(output, true);
    }
})

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

.Task("tests").Do(() =>
{
    if (!Directory.Exists(tests))
    {
        Directory.CreateDirectory(tests);
        System.Threading.Thread.Sleep(100); // HACK (adamralph): wait for the directory to be created
    }
})

.Exec("unit").DependsOn("build", "tests").Do(exec => exec
    .Run(xunitCommand)
    .With(unit, "/html", GetTestResultsPath(tests, unit, "html"), "/xml", GetTestResultsPath(tests, unit, "xml")))

.Task("output").Do(() =>
{
    if (!Directory.Exists(output))
    {
        Directory.CreateDirectory(output);
        System.Threading.Thread.Sleep(100); // HACK (adamralph): wait for the directory to be created
    }
})

.Task("pack").DependsOn("build", "clobber", "output").Do(() =>
{
    foreach (var pack in packs)
    {
        File.Copy(pack + ".nuspec", pack + ".nuspec.original", true);
    }

    try
    {
        foreach (var pack in packs)
        {
            File.WriteAllText(pack + ".nuspec", File.ReadAllText(pack + ".nuspec").Replace("0.0.0", version + versionSuffix));
            new Exec()
                .Run(nugetCommand)
                .With(
                    "pack", pack + ".csproj",
                    "-OutputDirectory", output,
                    "-Properties", "Configuration=Release",
                    "-IncludeReferencedProjects",
                    "-Verbosity " + nugetVerbosity)
                .Execute();
        }
    }
    finally
    {
        foreach (var pack in packs)
        {
            File.Copy(pack + ".nuspec.original", pack + ".nuspec", true);
            File.Delete(pack + ".nuspec.original");
        }
    }
})

.Run();

string GetTestResultsPath(string directory, string assembly, string extension)
{
    return Path.GetFullPath(
        Path.Combine(
            directory,
            string.Concat(
                Path.GetFileNameWithoutExtension(assembly),
                ".TestResults.",
                extension)));
}
