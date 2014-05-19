// <copyright file="MSBuild.GetPath.cs" company="Bau contributors">
//  Copyright (c) Bau contributors. (baubuildch@gmail.com)
// </copyright>

namespace BauMSBuild
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    public partial class MSBuild
    {
        private static string GetPath(
            string version,
            ProcessorArchitecture architecture,
            string windowsFolder,
            string programFilesFolder,
            bool is64BitOperatingSystem)
        {
            var upperVersion = version.ToUpperInvariant();
            switch (upperVersion)
            {
                case "VS4":
                case "VS40":
                case "VS":
                    throw CreateException(version, " ROFTL.");

                case "VS5":
                case "VS50":
                case "VS97":
                    throw CreateException(version, " LOL.");

                case "VS6":
                case "VS60":
                    throw CreateException(version, " The nineties called, and they want their IDE back.");

                case "NET1":
                case "NET10":
                case "NET11":

                case "VS7":
                case "VS70":
                case "VS2002":

                case "VS71":
                case "VS2003":
                    throw CreateException(version, " Only MSBuild 2.0 (Visual Studio 2005) or later is supported.");

                case "NET2":
                case "NET20":
                case "NET3":
                case "NET30":

                case "VS8":
                case "VS80":
                case "VS2005":
                    return GetFrameworkPath("v2.0.50727", architecture, windowsFolder, is64BitOperatingSystem);

                case "NET35":

                case "VS9":
                case "VS90":
                case "VS2008":
                    return GetFrameworkPath("v3.5", architecture, windowsFolder, is64BitOperatingSystem);

                case "NET4":
                case "NET40":
                case "NET45":

                case "VS10":
                case "VS2010":

                case "VS11":
                case "VS2012":
                    return GetFrameworkPath("v4.0.30319", architecture, windowsFolder, is64BitOperatingSystem);

                case "NET451":
                case "NET452":

                case "VS2013":
                    return GetVisualStudioPath("12.0", architecture, programFilesFolder, is64BitOperatingSystem);

                default:
                    if (upperVersion.StartsWith("VS", StringComparison.OrdinalIgnoreCase))
                    {
                        int numericVersion;
                        if (int.TryParse(upperVersion.Substring(2), out numericVersion) && numericVersion < 97)
                        {
                            var visualStudioVersion = numericVersion.ToString("F1", CultureInfo.InvariantCulture);
                            return GetVisualStudioPath(
                                visualStudioVersion, architecture, programFilesFolder, is64BitOperatingSystem);
                        }
                    }

                    throw CreateException(version, null);
            }
        }

        private static InvalidOperationException CreateException(string version, string hint)
        {
            var message = string.Format(
                CultureInfo.InvariantCulture, "MSBuild for '{0}' is not supported.{1}", version, hint);
            return new InvalidOperationException(message);
        }

        private static string GetVisualStudioPath(
            string subfolder, ProcessorArchitecture architecture, string programFilesFolder, bool is64BitOperatingSystem)
        {
            Guard.AgainstNullArgument("programFilesFolder", programFilesFolder);

            switch (architecture)
            {
                case ProcessorArchitecture.None:
                case ProcessorArchitecture.MSIL:
                    return is64BitOperatingSystem
                        ? Path.Combine(programFilesFolder, "MSBuild", subfolder, "bin", "amd64", "MSBuild.exe")
                        : Path.Combine(programFilesFolder, "MSBuild", subfolder, "bin", "MSBuild.exe");

                case ProcessorArchitecture.X86:
                    return Path.Combine(programFilesFolder, "MSBuild", subfolder, "bin", "MSBuild.exe");

                case ProcessorArchitecture.Amd64:
                    return Path.Combine(programFilesFolder, "MSBuild", subfolder, "bin", "amd64", "MSBuild.exe");

                default:
                    var message = string.Format(
                        CultureInfo.InvariantCulture,
                        "MSBuild processor architecture '{0}' is not supported.",
                        architecture.ToString());

                    throw new NotSupportedException(message);
            }
        }

        private static string GetFrameworkPath(
            string subfolder, ProcessorArchitecture architecture, string windowsFolder, bool is64BitOperatingSystem)
        {
            Guard.AgainstNullArgument("windowsFolder", windowsFolder);
            switch (architecture)
            {
                case ProcessorArchitecture.None:
                case ProcessorArchitecture.MSIL:
                    return is64BitOperatingSystem
                        ? Path.Combine(windowsFolder, "Microsoft.NET", "Framework64", subfolder, "MSBuild.exe")
                        : Path.Combine(windowsFolder, "Microsoft.NET", "Framework", subfolder, "MSBuild.exe");

                case ProcessorArchitecture.X86:
                    return Path.Combine(windowsFolder, "Microsoft.NET", "Framework", subfolder, "MSBuild.exe");

                case ProcessorArchitecture.Amd64:
                    return Path.Combine(windowsFolder, "Microsoft.NET", "Framework64", subfolder, "MSBuild.exe");

                default:
                    var message = string.Format(
                        CultureInfo.InvariantCulture,
                        "MSBuild processor architecture '{0}' is not supported.",
                        architecture.ToString());

                    throw new NotSupportedException(message);
            }
        }
    }
}
