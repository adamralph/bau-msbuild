// <copyright file="LoggerParameters.cs" company="Bau contributors">
//  Copyright (c) Bau contributors. (baubuildch@gmail.com)
// </copyright>

namespace BauMSBuild
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public abstract class LoggerParameters
    {
        public bool PerformanceSummary { get; set; }

        public bool Summary { get; set; }

        public bool NoSummary { get; set; }

        public bool ErrorsOnly { get; set; }

        public bool WarningsOnly { get; set; }

        public bool NoItemAndPropertyList { get; set; }

        public bool ShowCommandLine { get; set; }

        public bool ShowTimestamp { get; set; }

        public bool ShowEventId { get; set; }

        public bool ForceNoAlign { get; set; }

        public bool DisableConsoleColor { get; set; }

        public bool DisableMPLogging { get; set; }

        public bool EnableMPLogging { get; set; }

        public Verbosity? Verbosity { get; set; }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Following MSBuild docs.")]
        public virtual string Render()
        {
            var booleans = this.GetType().GetProperties()
                .Where(property => property.PropertyType == typeof(bool) && (bool)property.GetValue(this))
                .Select(property => property.Name)
                .OrderBy(name => name);

            var nullableBooleans = this.GetType().GetProperties()
                .Where(property => property.PropertyType == typeof(bool?))
                .Select(property => new { Name = property.Name, Value = (bool?)property.GetValue(this) })
                .Select(property => property.Value.HasValue
                    ? string.Concat(property.Name, "=", property.Value.Value.ToString().ToLowerInvariant())
                    : null)
                .OrderBy(name => name);

            var strings = this.GetType().GetProperties()
                .Where(property => property.PropertyType == typeof(string))
                .Select(property => string.Concat(property.Name, "=", (string)property.GetValue(this)))
                .OrderBy(name => name);

            var args = booleans.Concat(nullableBooleans).Concat(strings).Where(arg => !string.IsNullOrWhiteSpace(arg)).ToList();

            if (this.Verbosity.HasValue)
            {
                args.Add("verbosity=" + this.Verbosity.ToString().ToLowerInvariant());
            }

            return args.Any() ? string.Join(";", args) : null;
        }
    }
}
