﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// APIs related to .NET Core CLI.
    /// </summary>
    public static class DotNetCliContext
    {
        /// <summary>
        /// dotnet --verbose subcommand
        /// </summary>
        /// <returns></returns>
        public static bool IsGlobalVerbose()
        {
            bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_CLI_CONTEXT_VERBOSE"), out bool globalVerbose);
            return globalVerbose;
        }
    }
}