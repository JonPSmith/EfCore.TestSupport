// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// Old logging provider, but kept as some unit test code still uses it
    /// </summary>
    public class MyLoggerProvider : ILoggerProvider
    {
        private readonly LogLevel _logLevel;
        private readonly List<LogOutput> _logs;

        /// <summary>
        /// This is a logger provider that can be linked into a loggerFactory.
        /// It will capture the logs and place them as strings into the provided logs parameter
        /// </summary>
        /// <param name="logs">required: a an initialised List<string> to hold any logs that are created</string></param>
        /// <param name="logLevel">optional: the level from with you want to capture logs. Defaults to LogLevel.Information</param>
        public MyLoggerProvider(List<LogOutput> logs, LogLevel logLevel = LogLevel.Information)
        {
            _logs = logs;
            _logLevel = logLevel;
        }

        /// <summary>
        /// Create a logger
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            return new MyLogger(_logs, _logLevel);
        }

        /// <summary>
        /// Displose - does nothing
        /// </summary>
        public void Dispose()
        {
        }

        private class MyLogger : ILogger
        {
            private readonly LogLevel _logLevel;
            private readonly List<LogOutput> _logs;

            public MyLogger(List<LogOutput> logs, LogLevel logLevel)
            {
                _logs = logs;
                _logLevel = logLevel;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return logLevel >= _logLevel;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                _logs.Add( new LogOutput(logLevel, eventId, formatter(state, exception)));
                Console.WriteLine(formatter(state, exception));
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
}