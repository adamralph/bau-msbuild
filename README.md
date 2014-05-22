# ![Bau](https://raw.githubusercontent.com/bau-build/bau/dev/assets/bau.128.png).MSBuild

> Run MSBuild

[![Gitter chat](https://badges.gitter.im/bau-build/bau.png)](https://gitter.im/bau-build/bau) [![NuGet Status](http://img.shields.io/badge/NuGet-0.1.0~alpha-blue.svg?style=flat)](https://www.nuget.org/packages/Bau.MSBuild/)

## Install

```batch
> scriptcs -install Bau.MSBuild -pre
```

## Usage

### Examples

```C#
// execute the default version of MSBuild using default arguments and switches
bau.MSBuild("build");
```

```C#
// execute a specific version of MSBuild using custom arguments and switches
bau.MSBuild("build").Do(exec =>
{
    msbuild.MSBuildVersion = "net45";
	msbuild.MSBuildArchitecture = ProcessorArchitecture.X86;
    msbuild.Solution = solution;
    msbuild.Targets = new[] { "Clean", "Build", };
    msbuild.Properties = new { Configuration = "Release" };
});
```

#### Properties and arguments

- **MSBuildVersion** `string`: The version of MSBuild to use. Defaults to `vs12`. For a version which ships with Visual Studio 2013 or later, you can use a Visual Studio identifier, e.g. for Visual Studio 2013, use `vs12` or `vs2013`. For older versions which shipped with .NET framework releases, use a NuGet style framework identifier, e.g. `net45`.

- **MSBuildArchitecture** [System.Reflection.ProcessorArchitecture](http://msdn.microsoft.com/en-us/library/cc533013.aspx): Controls whether the 64-bit or 32-bit version of MSBuild is used. The default (`None`/`MSIL`) is the version matching the current OS. `IA64` and `Arm` are not supported. 

- **WorkingDirectory** `string`

- **Solution** *a.k.a ProjectFile* `string`: The solution or project to build. If not set, the default MSBuild behaviour occurs (see [MSBuild Command-Line Reference](http://msdn.microsoft.com/en-us/library/ms164311.aspx)).

#### Switches

The following properties map directly to [MSBuild command line switches](http://msdn.microsoft.com/en-us/library/ms164311.aspx). The documentation for each switch is not repeated here. For non-trivial mappings, an explanation is provided.

- **Help** `bool`

- **DetailedSummary** `bool`

- **IgnoreProjectExtensions** `IEnumerable<string>`

- **MaxCpuCount** `int?`: For unlimited CPU usage, set this to `-1` or `int.MaxValue`. 

- **NoAutoResponse** `bool`

- **NodeReuse** `bool?`

- **NoLogo** `bool`

- **Preprocess** `string`: To pass the switch with no argument, set this to `""` or `string.Empty`.

- **Properties** `object`: Set this to an instance of an anonymous type. E.g. `new { Configuration = "Release", WarningLevel = 2, }` will be rendered as the command line switches `"/property:Configuration=Release /property:WarningLevel=2"`. 

- **Targets** `IEnumerable<string>`

- **ToolsVersion** `string`

- **Validate** `string`: To pass the switch with no argument, set this to `""` or `string.Empty`.

- **Verbosity** `Verbosity`: `Normal` (default), `Quiet`, `Minimal`, `Detailed`, `Diagnostic`. 

- **Version** `bool`

- **ResponseFile** *a.k.a @* `string`

#### Switches for loggers

- **ConsoleLoggerParameters** `ConsoleLoggerParameters`: all properties are of type `bool` with the exception of **Verbosity** which is of type `Verbosity` (see above).

- **DistributedFileLogger** `bool`

- **DistributedLoggers** `IList<DistributedLogger>`: `DistributedLogger` has a **CentralLogger** `Logger` and **ForwardingLogger** `Logger`.  

- **FileLoggers** `IList<FileLogger>`: `FileLogger` has a **Number** `int` and a **FileLoggerParameters** `FileLoggerParameters`. 

- **Loggers** `IList<Logger>`

- **NoConsoleLogger** `bool`

#### Other

- **Args** `string`: This can be used to pass undocumented switches or is sometimes useful as a terse alternative to setting the properties described above. The value of **Args** is appended to the string rendered by any properties which have been set.
