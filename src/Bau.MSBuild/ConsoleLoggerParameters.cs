// <copyright file="ConsoleLoggerParameters.cs" company="Bau contributors">
//  Copyright (c) Bau contributors. (baubuildch@gmail.com)
// </copyright>

namespace BauMSBuild
{
    public class ConsoleLoggerParameters : LoggerParameters
    {
        public override string Render()
        {
            var args = base.Render();
            return string.IsNullOrWhiteSpace(args) ? null : "/consoleloggerparameters:" + args;
        }
    }
}
