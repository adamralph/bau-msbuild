// <copyright file="MSBuildFacts.cs" company="Bau contributors">
//  Copyright (c) Bau contributors. (baubuildch@gmail.com)
// </copyright>

namespace BauMSBuild.Test.Unit
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using FluentAssertions;
    using Xunit;
    using Xunit.Extensions;

    public static class MSBuildFacts
    {
        [Theory]
        [InlineData(null, "12.0")]

        [InlineData("net451", "12.0")]
        [InlineData("net452", "12.0")]

        [InlineData("vs12", "12.0")]
        [InlineData("vs2013", "12.0")]

        [InlineData("vs13", "13.0")]
        
        [InlineData("vs14", "14.0")]
        [InlineData("vs2015", "14.0")]
        public static void DeterminesVisualStudioFolder(string version, string subfolder)
        {
            // arrange
            var root = "root";
            var msbuild = new Derived { MSBuildVersion = version, MSBuildArchitecture = ProcessorArchitecture.Amd64 };
            var expectedFilename = Path.Combine(root, "MSBuild", subfolder, "bin", "amd64", "MSBuild.exe");

            // act
            var startInfo = msbuild.GetStartInfoVisualStudio(root);

            // assert
            startInfo.FileName.Should().Be(expectedFilename);
        }

        [Theory]
        [InlineData("net2", "v2.0.50727")]
        [InlineData("net20", "v2.0.50727")]
        [InlineData("net3", "v2.0.50727")]
        [InlineData("net30", "v2.0.50727")]

        [InlineData("vs8", "v2.0.50727")]
        [InlineData("vs80", "v2.0.50727")]
        [InlineData("vs2005", "v2.0.50727")]

        [InlineData("net35", "v3.5")]

        [InlineData("vs9", "v3.5")]
        [InlineData("vs90", "v3.5")]
        [InlineData("vs2008", "v3.5")]

        [InlineData("net4", "v4.0.30319")]
        [InlineData("net40", "v4.0.30319")]
        [InlineData("net45", "v4.0.30319")]

        [InlineData("vs10", "v4.0.30319")]
        [InlineData("vs2010", "v4.0.30319")]

        [InlineData("vs11", "v4.0.30319")]
        [InlineData("vs2012", "v4.0.30319")]
        public static void DeterminesFrameworkFolder(string version, string subfolder)
        {
            // arrange
            var root = "root";
            var msbuild = new Derived { MSBuildVersion = version, MSBuildArchitecture = ProcessorArchitecture.Amd64 };
            var expectedFilename = Path.Combine(root, "Microsoft.NET", "Framework64", subfolder, "MSBuild.exe");

            // act
            var startInfo = msbuild.GetStartInfoFramework(root);

            // assert
            startInfo.FileName.Should().Be(expectedFilename);
        }

        [Theory]
        [InlineData(ProcessorArchitecture.None, true, true)]
        [InlineData(ProcessorArchitecture.MSIL, true, true)]
        [InlineData(ProcessorArchitecture.X86, true, false)]
        [InlineData(ProcessorArchitecture.Amd64, true, true)]
        [InlineData(ProcessorArchitecture.None, false, false)]
        [InlineData(ProcessorArchitecture.MSIL, false, false)]
        [InlineData(ProcessorArchitecture.X86, false, false)]
        public static void DeterminesVisualStudioFolderPerArchitecture(
            ProcessorArchitecture architecture, bool is64BitOperatingSystem, bool expect64Bit)
        {
            // arrange
            var root = "root";
            var msbuild = new Derived { MSBuildVersion = "vs12", MSBuildArchitecture = architecture };
            var expectedFilename = expect64Bit
                ? Path.Combine(root, "MSBuild", "12.0", "bin", "amd64", "MSBuild.exe")
                : Path.Combine(root, "MSBuild", "12.0", "bin", "MSBuild.exe");

            // act
            var startInfo = msbuild.GetStartInfoVisualStudio(root, is64BitOperatingSystem);

            // assert
            startInfo.FileName.Should().Be(expectedFilename);
        }

        [Theory]
        [InlineData(ProcessorArchitecture.None, true, true)]
        [InlineData(ProcessorArchitecture.MSIL, true, true)]
        [InlineData(ProcessorArchitecture.X86, true, false)]
        [InlineData(ProcessorArchitecture.Amd64, true, true)]
        [InlineData(ProcessorArchitecture.None, false, false)]
        [InlineData(ProcessorArchitecture.MSIL, false, false)]
        [InlineData(ProcessorArchitecture.X86, false, false)]
        public static void DeterminesFrameworkFolderPerArchitecture(
            ProcessorArchitecture architecture, bool is64BitOperatingSystem, bool expect64Bit)
        {
            // arrange
            var root = "root";
            var msbuild = new Derived { MSBuildVersion = "net45", MSBuildArchitecture = architecture };
            var expectedFilename = expect64Bit
                ? Path.Combine(root, "Microsoft.NET", "Framework64", "v4.0.30319", "MSBuild.exe")
                : Path.Combine(root, "Microsoft.NET", "Framework", "v4.0.30319", "MSBuild.exe");

            // act
            var startInfo = msbuild.GetStartInfoFramework(root, is64BitOperatingSystem);

            // assert
            startInfo.FileName.Should().Be(expectedFilename);
        }

        [Theory]
        [InlineData("NET1")]
        [InlineData("NET10")]
        [InlineData("NET11")]

        [InlineData("VS7")]
        [InlineData("VS70")]
        [InlineData("VS2002")]

        [InlineData("VS71")]
        [InlineData("VS2003")]
        public static void GivesHintsForSupportedVisualStudioVersions(string version)
        {
            // arrange
            var msbuild = new Derived { MSBuildVersion = version, };

            // act
            var ex = Record.Exception(() => msbuild.GetStartInfoPublic());

            // assert
            ex.Should().BeOfType<InvalidOperationException>();
            ex.Message.Should().ContainEquivalentOf("MSBuild 2.0 (Visual Studio 2005) or later");
        }

        [Fact]
        public static void SetsWorkingDirectory()
        {
            // arrange
            var msbuild = new Derived { WorkingDirectory = "foo", };

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.WorkingDirectory.Should().Be(msbuild.WorkingDirectory);
        }

        [Fact]
        public static void DoesNotUseShellExecute()
        {
            // arrange
            var msbuild = new Derived();

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.UseShellExecute.Should().BeFalse();
        }

        [Fact]
        public static void SetsSolution()
        {
            // arrange
            var msbuild = new Derived { Solution = "Foo.sln", };
            var expectedArgs = "Foo.sln";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsHelp()
        {
            // arrange
            var msbuild = new Derived { Help = true };
            var expectedArgs = "/help";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsDetailedSummary()
        {
            // arrange
            var msbuild = new Derived { DetailedSummary = true, };
            var expectedArgs = "/detailedsummary";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsIgnoreProjectExtensions()
        {
            // arrange
            var msbuild = new Derived { IgnoreProjectExtensions = new[] { ".vcproj", ".sln" }, };
            var expectedArgs = "/ignoreprojectextensions:.vcproj,.sln";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsUnlimitedMaxCpuCount()
        {
            // arrange
            var msbuild = new Derived { MaxCpuCount = int.MaxValue, };
            var expectedArgs = "/maxcpucount";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsLimitedMaxCpuCount()
        {
            // arrange
            var msbuild = new Derived { MaxCpuCount = 8, };
            var expectedArgs = "/maxcpucount:8";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsNoAutoResponse()
        {
            // arrange
            var msbuild = new Derived { NoAutoResponse = true, };
            var expectedArgs = "/noautoresponse";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Theory]
        [InlineData(true, "/nodereuse:True")]
        [InlineData(false, "/nodereuse:False")]
        public static void SetsNodeReuse(bool nodeReuse, string expectedArgs)
        {
            // arrange
            var msbuild = new Derived { NodeReuse = nodeReuse, };

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsNoLogo()
        {
            // arrange
            var msbuild = new Derived { NoLogo = true, };
            var expectedArgs = "/nologo";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "/preprocess")]
        [InlineData(" ", "/preprocess")]
        [InlineData("foo.txt", "/preprocess:foo.txt")]
        public static void SetsPreprocess(string preprocess, string expectedArgs)
        {
            // arrange
            var msbuild = new Derived { Preprocess = preprocess, };

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsProperties()
        {
            // arrange
            var msbuild = new Derived
            {
                Properties = new
                {
                    Configuration = "Release",
                    WarningLevel = 2,
                },
            };

            var expectedArgs = "/property:Configuration=Release /property:WarningLevel=2";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsTargets()
        {
            // arrange
            var msbuild = new Derived
            {
                Targets = new[] { "Clean", "Build", }
            };

            var expectedArgs = "/target:Clean;Build";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsToolsVersion()
        {
            // arrange
            var msbuild = new Derived { ToolsVersion = "3.5" };
            var expectedArgs = "/toolsversion:3.5";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "/validate")]
        [InlineData(" ", "/validate")]
        [InlineData("foo.xsd", "/validate:foo.xsd")]
        public static void SetsValidate(string validate, string expectedArgs)
        {
            // arrange
            var msbuild = new Derived { Validate = validate, };

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Theory]
        [InlineData(Verbosity.Quiet, "/verbosity:quiet")]
        [InlineData(Verbosity.Minimal, "/verbosity:minimal")]
        [InlineData(Verbosity.Normal, "/verbosity:normal")]
        [InlineData(Verbosity.Detailed, "/verbosity:detailed")]
        [InlineData(Verbosity.Diagnostic, "/verbosity:diagnostic")]
        public static void SetsVerbosity(Verbosity verbosity, string expectedArgs)
        {
            // arrange
            var msbuild = new Derived { Verbosity = verbosity, };

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsVersion()
        {
            // arrange
            var msbuild = new Derived { Version = true, };
            var expectedArgs = "/version";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsResponseFile()
        {
            // arrange
            var msbuild = new Derived { ResponseFile = "foo.rsp", };
            var expectedArgs = "@foo.rsp";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsConsoleLoggerParameters()
        {
            // arrange
            var msbuild = new Derived
            {
                ConsoleLoggerParameters = new ConsoleLoggerParameters
                {
                    DisableConsoleColor = true,
                    EnableMPLogging = true,
                    DisableMPLogging = true,
                    ErrorsOnly = true,
                    ForceNoAlign = true,
                    NoItemAndPropertyList = true,
                    NoSummary = true,
                    PerformanceSummary = true,
                    ShowCommandLine = true,
                    ShowEventId = true,
                    ShowTimestamp = true,
                    Summary = true,
                    Verbosity = Verbosity.Minimal,
                    WarningsOnly = true,
                }
            };

            var expectedArgs = "/consoleloggerparameters:DisableConsoleColor;DisableMPLogging;EnableMPLogging;" +
                "ErrorsOnly;ForceNoAlign;NoItemAndPropertyList;NoSummary;PerformanceSummary;ShowCommandLine;" +
                "ShowEventId;ShowTimestamp;Summary;WarningsOnly;verbosity=minimal";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsDistributedFileLogger()
        {
            // arrange
            var msbuild = new Derived { DistributedFileLogger = true, };
            var expectedArgs = "/distributedFileLogger";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsDistributedLoggers()
        {
            // arrange
            var msbuild = new Derived();
            var logger1 = new DistributedLogger(
                new Logger("C1.dll") { LoggerClass = "C1.Foo", LoggerParameters = "C1Bar;C1Baz" },
                new Logger("F1.dll"));

            var logger2 = new DistributedLogger(
                new Logger("C2.dll") { LoggerClass = "C2.Foo" },
                new Logger("F2.dll") { LoggerParameters = "F2Bar;F2Baz" });

            msbuild.DistributedLoggers.Add(logger1);
            msbuild.DistributedLoggers.Add(logger2);

            var expectedArgs =
                "/distributedlogger:C1.Foo,C1.dll;C1Bar;C1Baz*F1.dll " +
                "/distributedlogger:C2.Foo,C2.dll*F2.dll;F2Bar;F2Baz";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsFileLoggerParameters()
        {
            // arrange
            var msbuild = new Derived();

            // defaults
            msbuild.FileLoggers.Add(new FileLogger());

            // default parameters
            msbuild.FileLoggers.Add(new FileLogger
            {
                FileLoggerParameters = new FileLoggerParameters(),
            });

            // no number
            msbuild.FileLoggers.Add(new FileLogger
            {
                Number = null,
                FileLoggerParameters = new FileLoggerParameters
                {
                    DisableConsoleColor = true,
                    EnableMPLogging = true,
                    DisableMPLogging = true,
                    ErrorsOnly = true,
                    ForceNoAlign = true,
                    NoItemAndPropertyList = true,
                    NoSummary = true,
                    PerformanceSummary = true,
                    ShowCommandLine = true,
                    ShowEventId = true,
                    ShowTimestamp = true,
                    Summary = true,
                    Verbosity = Verbosity.Minimal,
                    WarningsOnly = true,
                    Append = true,
                    LogFile = "logfile.txt",
                    Encoding = "UTF-8",
                }
            });

            // the lot
            msbuild.FileLoggers.Add(new FileLogger
            {
                Number = 1,
                FileLoggerParameters = new FileLoggerParameters
                {
                    DisableConsoleColor = true,
                    EnableMPLogging = true,
                    DisableMPLogging = true,
                    ErrorsOnly = true,
                    ForceNoAlign = true,
                    NoItemAndPropertyList = true,
                    NoSummary = true,
                    PerformanceSummary = true,
                    ShowCommandLine = true,
                    ShowEventId = true,
                    ShowTimestamp = true,
                    Summary = true,
                    Verbosity = Verbosity.Minimal,
                    WarningsOnly = true,
                    Append = false,
                    LogFile = "logfile1.txt",
                    Encoding = "Unicode",
                }
            });

            msbuild.FileLoggers.Add(new FileLogger
            {
                Number = 2,
                FileLoggerParameters = new FileLoggerParameters
                {
                    // NOTE (adamralph): no Append
                    DisableConsoleColor = true,
                    EnableMPLogging = true,
                    DisableMPLogging = true,
                    ErrorsOnly = true,
                    ForceNoAlign = true,
                    NoItemAndPropertyList = true,
                    NoSummary = true,
                    PerformanceSummary = true,
                    ShowCommandLine = true,
                    ShowEventId = true,
                    ShowTimestamp = true,
                    Summary = true,
                    Verbosity = Verbosity.Minimal,
                    WarningsOnly = true,
                    LogFile = "logfile2.txt",
                    Encoding = "ASCII",
                }
            });

            msbuild.FileLoggers.Add(new FileLogger
            {
                Number = 3,
            });

            var expectedArgs =
                "/fileLogger" +
                " " +
                "/fileLogger" +
                " " +
                "/fileLogger /fileloggerparameters:DisableConsoleColor;DisableMPLogging;EnableMPLogging;" +
                "ErrorsOnly;ForceNoAlign;NoItemAndPropertyList;NoSummary;PerformanceSummary;ShowCommandLine;" +
                "ShowEventId;ShowTimestamp;Summary;WarningsOnly;Append=true;Encoding=UTF-8;LogFile=logfile.txt;verbosity=minimal" +
                " " +
                "/fileLogger1 /fileloggerparameters1:DisableConsoleColor;DisableMPLogging;EnableMPLogging;" +
                "ErrorsOnly;ForceNoAlign;NoItemAndPropertyList;NoSummary;PerformanceSummary;ShowCommandLine;" +
                "ShowEventId;ShowTimestamp;Summary;WarningsOnly;Append=false;Encoding=Unicode;LogFile=logfile1.txt;verbosity=minimal" +
                " " +
                "/fileLogger2 /fileloggerparameters2:DisableConsoleColor;DisableMPLogging;EnableMPLogging;" +
                "ErrorsOnly;ForceNoAlign;NoItemAndPropertyList;NoSummary;PerformanceSummary;ShowCommandLine;" +
                "ShowEventId;ShowTimestamp;Summary;WarningsOnly;Encoding=ASCII;LogFile=logfile2.txt;verbosity=minimal" +
                " " +
                "/fileLogger3";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsLoggers()
        {
            // arrange
            var msbuild = new Derived();
            msbuild.Loggers.Add(new Logger("C1.dll") { LoggerClass = "C1.Foo", LoggerParameters = "C1Bar;C1Baz" });
            msbuild.Loggers.Add(new Logger("F1.dll"));
            msbuild.Loggers.Add(new Logger("C2.dll") { LoggerClass = "C2.Foo" });
            msbuild.Loggers.Add(new Logger("F2.dll") { LoggerParameters = "F2Bar;F2Baz" });

            var expectedArgs =
                "/logger:C1.Foo,C1.dll;C1Bar;C1Baz " +
                "/logger:F1.dll " +
                "/logger:C2.Foo,C2.dll " +
                "/logger:F2.dll;F2Bar;F2Baz";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        [Fact]
        public static void SetsNoConsoleLogger()
        {
            // arrange
            var msbuild = new Derived { NoConsoleLogger = true };
            var expectedArgs = "/noconsolelogger";

            // act
            var startInfo = msbuild.GetStartInfoPublic();

            // assert
            startInfo.Arguments.Should().Be(expectedArgs);
        }

        private class Derived : MSBuild
        {
            public ProcessStartInfo GetStartInfoPublic()
            {
                return this.GetStartInfo("Windows", "ProgramFiles", true);
            }

            public ProcessStartInfo GetStartInfoFramework(string windowsFolder)
            {
                return this.GetStartInfo(windowsFolder, "ProgramFiles", true);
            }

            public ProcessStartInfo GetStartInfoVisualStudio(string programFilesFolder)
            {
                return this.GetStartInfo("Windows", programFilesFolder, true);
            }

            public ProcessStartInfo GetStartInfoFramework(string windowsFolder, bool is64BitOperatingSystem)
            {
                return this.GetStartInfo(windowsFolder, "ProgramFiles", is64BitOperatingSystem);
            }

            public ProcessStartInfo GetStartInfoVisualStudio(string programFilesFolder, bool is64BitOperatingSystem)
            {
                return this.GetStartInfo("Windows", programFilesFolder, is64BitOperatingSystem);
            }
        }
    }
}
