// <copyright file="Plugin.cs" company="Bau contributors">
//  Copyright (c) Bau contributors. (baubuildch@gmail.com)
// </copyright>

namespace BauMSBuild
{
    using BauCore;

    public static class Plugin
    {
        public static ITaskBuilder<MSBuild> MSBuild(this ITaskBuilder builder, string name = null)
        {
            return new TaskBuilder<MSBuild>(builder, name);
        }
    }
}
