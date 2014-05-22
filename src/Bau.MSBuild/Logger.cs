// <copyright file="Logger.cs" company="Bau contributors">
//  Copyright (c) Bau contributors. (baubuildch@gmail.com)
// </copyright>

namespace BauMSBuild
{
    using System;

    public class Logger
    {
        private readonly string loggerAssembly;

        public Logger(string loggerAssembly)
        {
            if (string.IsNullOrWhiteSpace(loggerAssembly))
            {
                throw new ArgumentException("loggerAssembly is null or whitespace.");
            }

            this.loggerAssembly = loggerAssembly;
        }

        public string LoggerClass { get; set; }

        public string LoggerAssembly
        {
            get { return this.loggerAssembly; }
        }

        public string LoggerParameters { get; set; }

        public string Render()
        {
            var @class = string.IsNullOrWhiteSpace(this.LoggerClass)
                ? null
                : this.LoggerClass + ",";

            var parameters = string.IsNullOrWhiteSpace(this.LoggerParameters)
                ? null
                : ";" + this.LoggerParameters;

            return string.Concat(@class, this.loggerAssembly, parameters);
        }
    }
}
