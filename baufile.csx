var isMono = Type.GetType("Mono.Runtime") != null;

// parameters
var versionSuffix = Environment.GetEnvironmentVariable("VERSION_SUFFIX") ?? "-adhoc";
var msBuildFileVerbosity = (Verbosity)Enum.Parse(typeof(Verbosity), Environment.GetEnvironmentVariable("MSBUILD_FILE_VERBOSITY") ?? "minimal", true);
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
var pack = "src/Bau.MSBuild/Bau.MSBuild";

// solution agnostic tasks
var bau = Require<Bau>();

bau
.Task("default").DependsOn("unit");

if(!isMono)
{
	bau.Task("default").DependsOn("pack");
}

bau
.Task("logs").Do(() => CreateDirectory(logs));

Action<string, string, string, string, string> csproj = (string projectName, string location, string name, string target, string logFile) =>
{
	if (isMono)
	{
	    bau.Exec(projectName + ":" + name).Do(exec => exec
	        .Run("xbuild")
	        .With(location + ".csproj", "/target:" + target, "/property:Configuration=Release", "/verbosity:normal", "/nologo"));
	}
	else
	{
	    bau.MSBuild(projectName + ":" + name).Do(msb =>
	    {
		    msb.MSBuildVersion = "net45";
		    msb.Properties = new { Configuration = "Release" };
		    msb.MaxCpuCount = -1;
		    msb.NodeReuse = false;
		    msb.Verbosity = Verbosity.Minimal;
		    msb.NoLogo = true;
		    msb.FileLoggers.Add(
		        new FileLogger
		        {
		            FileLoggerParameters = new FileLoggerParameters
		            {
		                PerformanceSummary = true,
		                Summary = true,
		                Verbosity = msBuildFileVerbosity,
		                LogFile = logs + "/" + logFile,
		            }
		        });
	        msb.Solution = location + ".csproj";
	        msb.Targets = new[] { target, };
	    });
	}
};

Action<string, string> csharpClean = (string projectName, string location) =>
{
	csproj(projectName, location, "clean", "Clean", "clean.log");
	var cleanTaskName = bau.CurrentTask.Name;
	bau.Task(cleanTaskName).DependsOn("logs");
	bau.Task("clean").DependsOn(cleanTaskName);
};

Action<string, string> csharpBuild = (string projectName, string location) =>
{
	csproj(projectName, location, "build", "Build", "build.log");
	var buildTaskName = bau.CurrentTask.Name;
	bau.Task(buildTaskName).DependsOn("restore", "logs");
	bau.Task("build").DependsOn(buildTaskName);
};

Action<string, string> csharp = (string projectName, string location) =>
{

	csharpClean(projectName, location);
	csharpBuild(projectName, location);

	if(File.Exists(location + ".nuspec"))
	{
		bau.Exec(projectName + ":pack").DependsOn("build", "clobber", "output").Do(exec => exec
					.Run(nugetCommand)
	                .With(
	                    "pack", location + ".csproj",
	                    "-Version", version + versionSuffix,
	                    "-OutputDirectory", output,
	                    "-Properties", "Configuration=Release",
	                    "-IncludeReferencedProjects",
	                    "-Verbosity " + nugetVerbosity));

		bau.Task("pack").DependsOn(projectName + ":pack");
	}
};

bau.Task("build").DependsOn("clean", "restore");

csharp("core", "src/Bau.MSBuild/Bau.MSBuild");
csharp("test:unit", "src/test/Bau.MSBuild.Test.Unit/Bau.MSBuild.Test.Unit");

bau
.Task("clobber").DependsOn("clean").Do(() => DeleteDirectory(output));

if (isMono)
{
    bau.Exec("restore").Do(exec => exec
        .Run("mono")
        .With(new [] { nugetCommand, "restore", solution }));
}
else
{
    bau.Exec("restore").Do(exec => exec
        .Run(nugetCommand)
        .With(new [] { "restore", solution }));
}

bau
.Task("tests").Do(() => CreateDirectory(tests))

.Xunit("unit").DependsOn("build", "tests").Do(xunit => xunit
    .Use(xunitCommand).Run(unit).Html().Xml())

.Task("output").Do(() => CreateDirectory(output))

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
