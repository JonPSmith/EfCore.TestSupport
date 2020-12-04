// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TestSupport.EfHelpers
{
    public class LogToOptions
    {
        /// <summary>
        /// This sets the lowest LogLevel that will be returned
        /// Defaults to LogLevel.Information
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// This allows you to only show certain DbLoggerCategory, for instance
        /// new[] { DbLoggerCategory.Database.Command.Name }) would only show the Database.Command logs
        /// Defaults to null, i.e. not used 
        /// </summary>
        public string[] OnlyShowTheseCategories { get; set; }

        /// <summary>
        /// This allows you to only show certain events, for instance
        /// new[] { CoreEventId.ContextInitialized }
        /// Defaults to null, i.e. not used 
        /// </summary>
        public EventId[] OnlyShowTheseEvents { get; set; }

        /// <summary>
        /// This allows you to set format of the log message, for instance
        /// DefaultWithUtcTime will use a UTC time instead the local time
        /// Defaults to null, which uses local time
        /// </summary>
        public DbContextLoggerOptions LoggerOptions { get; set; }

        internal DbContextOptionsBuilder<T> AddLogTo<T>(DbContextOptionsBuilder<T> builder, Action<string> action)
            where T : DbContext
        {
            if (OnlyShowTheseCategories != null)
                return builder.LogTo(action, OnlyShowTheseCategories, LogLevel, LoggerOptions);
            if (OnlyShowTheseEvents != null)
                return builder.LogTo(action, OnlyShowTheseEvents, LogLevel, LoggerOptions);

            return builder.LogTo(action, LogLevel, LoggerOptions);
        }

    }
}