// <copyright file="MSBuild.cs" company="Bau contributors">
//  Copyright (c) Bau contributors. (baubuildch@gmail.com)
// </copyright>

namespace BauMSBuild
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using BauCore;

    public partial class MSBuild : BauTask
    {
        private readonly IList<DistributedLogger> distributedLoggers = new List<DistributedLogger>();
        private readonly IList<FileLogger> fileLoggers = new List<FileLogger>();
        private readonly IList<Logger> loggers = new List<Logger>();

        public string WorkingDirectory { get; set; }

        public string MSBuildVersion { get; set; }

        public ProcessorArchitecture? MSBuildArchitecture { get; set; }

        public string Solution { get; set; }

        public bool Help { get; set; }

        public bool DetailedSummary { get; set; }

        public IEnumerable<string> IgnoreProjectExtensions { get; set; }

        public int? MaxCpuCount { get; set; }

        public bool NoAutoResponse { get; set; }

        public bool? NodeReuse { get; set; }

        public bool NoLogo { get; set; }

        public string Preprocess { get; set; }

        public object Properties { get; set; }

        public IEnumerable<string> Targets { get; set; }

        public string ToolsVersion { get; set; }

        public string Validate { get; set; }

        public Verbosity? Verbosity { get; set; }

        public bool Version { get; set; }

        public string ResponseFile { get; set; }

        public ConsoleLoggerParameters ConsoleLoggerParameters { get; set; }

        public bool DistributedFileLogger { get; set; }

        public IList<DistributedLogger> DistributedLoggers
        {
            get { return this.distributedLoggers; }
        }

        public IList<FileLogger> FileLoggers
        {
            get { return this.fileLoggers; }
        }

        public IList<Logger> Loggers
        {
            get { return this.loggers; }
        }

        public bool NoConsoleLogger { get; set; }

        public string Args { get; set; }

        protected override void OnActionsExecuted()
        {
            var info = this.GetStartInfo(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Environment.Is64BitOperatingSystem);

            var argString = string.IsNullOrEmpty(info.Arguments)
                ? string.Empty
                : " " + string.Join(" ", info.Arguments);

            var workingDirectoryString = string.IsNullOrEmpty(info.WorkingDirectory)
                ? string.Empty
                : string.Format(CultureInfo.InvariantCulture, " with working directory '{0}'", info.WorkingDirectory);

            Console.WriteLine("Executing '{0}{1}'{2}...", info.FileName, argString, workingDirectoryString);

            info.Run();
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "It's not complicated, just tedious.")]
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Following MSBuild docs.")]
        protected ProcessStartInfo GetStartInfo(
            string windowsFolder, string programFilesFolder, bool is64BitOperatingSystem)
        {
            var args = new List<string>();

            if (!string.IsNullOrWhiteSpace(this.Solution))
            {
                args.Add(this.Solution);
            }

            if (this.Help)
            {
                args.Add("/help");
            }

            if (this.DetailedSummary)
            {
                args.Add("/detailedsummary");
            }

            if (this.IgnoreProjectExtensions != null)
            {
                var array = this.IgnoreProjectExtensions.ToArray();
                if (array.Length != 0)
                {
                    args.Add("/ignoreprojectextensions:" + string.Join(",", this.IgnoreProjectExtensions));
                }
            }

            if (this.MaxCpuCount.HasValue)
            {
                args.Add("/maxcpucount" +
                    (this.MaxCpuCount.Value > 1024 || this.MaxCpuCount.Value < 1
                        ? null
                        : ":" + this.MaxCpuCount.Value.ToString(CultureInfo.InvariantCulture)));
            }

            if (this.NoAutoResponse)
            {
                args.Add("/noautoresponse");
            }

            if (this.NodeReuse.HasValue)
            {
                args.Add("/nodereuse:" + this.NodeReuse.ToString());
            }

            if (this.NoLogo)
            {
                args.Add("/nologo");
            }

            if (this.Preprocess != null)
            {
                args.Add("/preprocess" + (string.IsNullOrWhiteSpace(this.Preprocess) ? null : ":" + this.Preprocess));
            }

            if (this.Properties != null)
            {
                args.AddRange(this.Properties.GetType().GetProperties().Select(property =>
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "/property:{0}={1}",
                        property.Name,
                        property.GetValue(this.Properties))));
            }

            if (this.Targets != null)
            {
                args.Add("/target:" + string.Join(";", this.Targets));
            }

            if (!string.IsNullOrWhiteSpace(this.ToolsVersion))
            {
                args.Add("/toolsversion:" + this.ToolsVersion);
            }

            if (this.Validate != null)
            {
                args.Add("/validate" + (string.IsNullOrWhiteSpace(this.Validate) ? null : ":" + this.Validate));
            }

            if (this.Verbosity.HasValue)
            {
                args.Add("/verbosity:" + this.Verbosity.ToString().ToLowerInvariant());
            }

            if (this.Version)
            {
                args.Add("/version");
            }

            if (!string.IsNullOrWhiteSpace(this.ResponseFile))
            {
                args.Add("@" + this.ResponseFile);
            }

            if (!string.IsNullOrWhiteSpace(this.Args))
            {
                args.Add(this.Args);
            }

            if (this.ConsoleLoggerParameters != null)
            {
                args.Add(this.ConsoleLoggerParameters.Render());
            }

            if (this.DistributedFileLogger)
            {
                args.Add("/distributedFileLogger");
            }

            args.AddRange(this.distributedLoggers.Select(logger => logger.Render()));
            args.AddRange(this.fileLoggers.Select(logger => logger.Render()));
            args.AddRange(this.loggers.Select(logger => "/logger:" + logger.Render()));

            if (this.NoConsoleLogger)
            {
                args.Add("/noconsolelogger");
            }

            return new ProcessStartInfo
            {
                FileName = GetPath(
                    this.MSBuildVersion ?? "vs2013",
                    this.MSBuildArchitecture.HasValue ? this.MSBuildArchitecture.Value : ProcessorArchitecture.None,
                    windowsFolder,
                    programFilesFolder,
                    is64BitOperatingSystem),

                Arguments = string.Join(" ", args.Where(arg => !string.IsNullOrWhiteSpace(arg))),
                WorkingDirectory = this.WorkingDirectory,
                UseShellExecute = false,
            };
        }
    }
}
