﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// The exception that is thrown when command line arguments could not be parsed.
    /// </summary>
    public class CommandParsingException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="CommandParsingException"/>.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="message">The message.</param>
        public CommandParsingException(CommandLineApplication command, string message)
            : base(message)
        {
            Command = command;
        }

        /// <summary>
        /// The command that is throwing the exception.
        /// </summary>
        public CommandLineApplication Command { get; }
    }
}
