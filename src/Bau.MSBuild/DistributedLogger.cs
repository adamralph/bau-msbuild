// <copyright file="DistributedLogger.cs" company="Bau contributors">
//  Copyright (c) Bau contributors. (baubuildch@gmail.com)
// </copyright>

namespace BauMSBuild
{
    using System;

    public class DistributedLogger
    {
        private readonly Logger centralLogger;

        private readonly Logger forwardingLogger;

        public DistributedLogger(Logger centralLogger, Logger forwardingLogger)
        {
            if (centralLogger == null)
            {
                throw new ArgumentNullException("centralLogger");
            }

            if (forwardingLogger == null)
            {
                throw new ArgumentNullException("forwardingLogger");
            }

            this.centralLogger = centralLogger;
            this.forwardingLogger = forwardingLogger;
        }

        public Logger CentralLogger
        {
            get { return this.centralLogger; }
        }

        public Logger ForwardingLogger
        {
            get { return this.forwardingLogger; }
        }

        public string Render()
        {
            return string.Concat("/distributedlogger:", this.centralLogger.Render(), "*", this.forwardingLogger.Render());
        }
    }
}
