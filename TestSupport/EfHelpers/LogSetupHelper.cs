// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

        public static IEnumerable<string> DecodedLogs(List<LogOutput> logs)
        {
            var indexOfSenStart = logs.IndexOf(logs.FirstOrDefault(x =>
                x?.EventId.Name == CoreEventId.SensitiveDataLoggingEnabledWarning.Name));
            if (indexOfSenStart < 0)
                indexOfSenStart = int.MaxValue; //this will cause all logs to be returned without decoding

            //There are sensitive logs, so decode params
            for (int i = 0; i < logs.Count; i++)
            {
                if (i < indexOfSenStart || logs[i]?.EventId.Name != RelationalEventId.CommandExecuted.Name)
                    yield return logs[i].ToString();

                yield return InternalDecodeLog(logs[i]);
            }
        }

        private static string InternalDecodeLog(LogOutput log)
        {
            string pattern = @"(@.*?)='";
            //see https://msdn.microsoft.com/en-us/library/system.text.regularexpressions.capture(v=vs.110)
            //Abandoned as have to finish the chapter
            throw new NotImplementedException();
        }
    }
}