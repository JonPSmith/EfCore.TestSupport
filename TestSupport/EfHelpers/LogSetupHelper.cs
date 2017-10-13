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
        /// <summary>
        /// This extension method will start logging the output of EF Core
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logLevel"></param>
        /// <returns>It passes back a reference to a list of Logs that EF Core will add to as the unit test progresses</returns>
        public static List<LogOutput> SetupLogging( //#A
            this DbContext context, //#B
            LogLevel logLevel = LogLevel.Information) //#C
        {
            var logs = new List<LogOutput>(); //#D
            var loggerFactory = context        //#E
                .GetService<ILoggerFactory>(); //#E
            loggerFactory.AddProvider(                 //#F
                new MyLoggerProvider(logs, logLevel)); //#F
            return logs; //#G
        }
        /********************************************************************
        #A This is an extension method that will start capturing logging produced by EF Core
        #B I need the current application's DbContext to work with
        #C It default to capturing logs at Information level or above, but you can change that
        #D I create a list of LogOutput, which is class I created to hold the 
        #E I get the ILoggerFactory service from EF Core
        #D
         * *******************************************************************/

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