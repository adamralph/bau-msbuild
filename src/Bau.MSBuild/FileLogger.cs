// <copyright file="FileLogger.cs" company="Bau contributors">
//  Copyright (c) Bau contributors. (baubuildch@gmail.com)
// </copyright>

namespace BauMSBuild
{
    using System.Globalization;

    public class FileLogger
    {
        public int? Number { get; set; }

        public FileLoggerParameters FileLoggerParameters { get; set; }

        public string Render()
        {
            return string.Concat(
                "/fileLogger",
                this.Number.HasValue ? this.Number.Value.ToString(CultureInfo.InvariantCulture) : null,
                this.FileLoggerParameters == null ? null : " " + this.FileLoggerParameters.Render(this.Number));
        }
    }
}
