// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace TestSupport.EfHelpers
{
    public class MyLoggerProvider : ILoggerProvider
    {
        private readonly List<LogOutput> _logs;
        private readonly LogLevel _logLevel;

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

        public ILogger CreateLogger(string categoryName)
        {
            return new MyLogger(_logs, _logLevel);
        }

        public void Dispose()
        {
        }

        private class MyLogger : ILogger
        {
            private readonly List<LogOutput> _logs;
            private readonly LogLevel _logLevel;

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