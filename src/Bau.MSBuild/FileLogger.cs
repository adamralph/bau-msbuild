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
            var parameters = this.FileLoggerParameters == null ? null : this.FileLoggerParameters.Render(this.Number);

            return string.Concat(
                "/fileLogger",
                this.Number.HasValue ? this.Number.Value.ToString(CultureInfo.InvariantCulture) : null,
                string.IsNullOrWhiteSpace(parameters) ? null : " " + parameters);
        }
    }
}
