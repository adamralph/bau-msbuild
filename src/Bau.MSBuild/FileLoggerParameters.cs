// <copyright file="FileLoggerParameters.cs" company="Bau contributors">
//  Copyright (c) Bau contributors. (baubuildch@gmail.com)
// </copyright>

namespace BauMSBuild
{
    using System.Globalization;

    public class FileLoggerParameters : LoggerParameters
    {
        public string LogFile { get; set; }

        public bool? Append { get; set; }

        public string Encoding { get; set; }

        public string Render(int? number)
        {
            var args = base.Render();
            return string.IsNullOrWhiteSpace(args)
                ? null
                : string.Concat(
                    "/fileloggerparameters",
                    number.HasValue ? number.Value.ToString(CultureInfo.InvariantCulture) : null,
                    ":",
                    args);
        }
    }
}
