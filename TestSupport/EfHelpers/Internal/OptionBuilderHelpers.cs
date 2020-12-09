// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
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

        internal static DbContextOptionsBuilder<T> AddLogTo<T>(this DbContextOptionsBuilder<T> builder, Action<string> action, LogToOptions logToOptions)
            where T : DbContext
        {
            logToOptions ??= new LogToOptions();

            if (logToOptions.OnlyShowTheseCategories != null && logToOptions.OnlyShowTheseEvents != null)
                throw new NotImplementedException($"You can't define a {nameof(LogToOptions.OnlyShowTheseCategories)} and {nameof(LogToOptions.OnlyShowTheseEvents)} at the same time");

            if (logToOptions.OnlyShowTheseCategories != null)
                return builder.LogTo(action, logToOptions.OnlyShowTheseCategories, logToOptions.LogLevel, logToOptions.LoggerOptions);
            if (logToOptions.OnlyShowTheseEvents != null)
                return builder.LogTo(action, logToOptions.OnlyShowTheseEvents, logToOptions.LogLevel, logToOptions.LoggerOptions);

            return builder.LogTo(action, logToOptions.LogLevel, logToOptions.LoggerOptions);
        }
    }
}