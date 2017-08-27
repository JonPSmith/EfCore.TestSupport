// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace TestSupport.EfHelpers
{
    public static class LogSetupHelper
    {
        public static List<LogOutput> SetupLogging(this DbContext context, LogLevel logLevel = LogLevel.Information)
        {
            var logs = new List<LogOutput>();
            var loggerFactory = context.GetService<ILoggerFactory>();
            loggerFactory.AddProvider(new MyLoggerProvider(logs, logLevel));
            return logs;
        }
    }
}