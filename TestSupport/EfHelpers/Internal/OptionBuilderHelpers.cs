// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestSupport.EfHelpers.Internal
{

    internal static class OptionBuilderHelpers
    {
        internal static void ApplyOtherOptionSettings<T>(this DbContextOptionsBuilder<T> builder)
            where T : DbContext
        {
            builder
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging();
        }

        internal static DbContextOptionsBuilder<T> AddLogTo<T>(this DbContextOptionsBuilder<T> builder, Action<string> userAction, LogToOptions logToOptions)
            where T : DbContext
        {
            logToOptions ??= new LogToOptions();

            Action<string> action = log =>
            {
                if (logToOptions.ShowLog)
                    userAction(log);
            };

            var usedNames = new List<string>();

            if (logToOptions.OnlyShowTheseCategories != null)
                usedNames.Add(nameof(LogToOptions.OnlyShowTheseCategories));
            if (logToOptions.OnlyShowTheseEvents != null)
                usedNames.Add(nameof(LogToOptions.OnlyShowTheseEvents));
            if (logToOptions.FilterFunction != null)
                usedNames.Add(nameof(LogToOptions.FilterFunction));

            if (usedNames.Count > 1)
                throw new NotSupportedException($"You can't define {string.Join(" and ", usedNames)} at the same time.");

            if (logToOptions.OnlyShowTheseCategories != null)
                return builder.LogTo(action, logToOptions.OnlyShowTheseCategories, logToOptions.LogLevel, logToOptions.LoggerOptions);
            if (logToOptions.OnlyShowTheseEvents != null)
                return builder.LogTo(action, logToOptions.OnlyShowTheseEvents, logToOptions.LogLevel, logToOptions.LoggerOptions);
            if (logToOptions.FilterFunction != null)
                return builder.LogTo(action, logToOptions.FilterFunction, logToOptions.LoggerOptions);

            return builder.LogTo(action, logToOptions.LogLevel, logToOptions.LoggerOptions);
        }
    }
}