// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// This class allows you to define the various LogTo options
    /// NOTE: You can only set a value for one of the three filters: OnlyShow... and FilterFunction
    /// </summary>
    public class LogToOptions
    {
        /// <summary>
        /// This controls the action being called. If set to false it will not call the action
        /// Defaults to true, i.e. returns all logs
        /// </summary>
        public bool ShowLog { get; set; } = true;

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
        /// This allows you to provide a method to filter the logs, for instance
        /// bool MyFilterFunction(EventId eventId, LogLevel logLevel) {...}
        /// Defaults to null, i.e. not used
        /// </summary>
        public Func<EventId, LogLevel, bool> FilterFunction { get; set; }

        /// <summary>
        /// This allows you to set format of the log message, for instance
        /// DefaultWithUtcTime will use a UTC time instead the local time
        /// Defaults to None, which means no extra info is prepended to the message
        /// </summary>
        public DbContextLoggerOptions LoggerOptions { get; set; } = DbContextLoggerOptions.None;
    }
}