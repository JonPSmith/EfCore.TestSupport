// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Logging;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// This 
    /// </summary>
    public class MyLoggerProviderActionOut : ILoggerProvider
    {
        private readonly Action<LogOutput> _efLog;
        private readonly LogLevel _logLevel;

        /// <summary>
        /// This is a logger provider that can be linked into a loggerFactory.
        /// It will capture the logs and place them as strings into the provided logs parameter
        /// </summary>
        /// <param name="efLog">required: a method that will be called when EF Core logs something</param>
        /// <param name="logLevel">optional: the level from with you want to capture logs. Defaults to LogLevel.Information</param>
        public MyLoggerProviderActionOut(Action<LogOutput> efLog, LogLevel logLevel = LogLevel.Information)
        {
            _efLog = efLog;
            _logLevel = logLevel;
        }

        /// <summary>
        /// Create a logger that will return a log when it is called.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            return new MyLogger(_efLog, _logLevel);
        }

        /// <summary>
        /// Dispose - not used
        /// </summary>
        public void Dispose()
        {
        }

        private class MyLogger : ILogger
        {
            private readonly Action<LogOutput> _efLog;
            private readonly LogLevel _logLevel;

            public MyLogger(Action<LogOutput> efLog, LogLevel logLevel)
            {
                _efLog = efLog;
                _logLevel = logLevel;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return logLevel >= _logLevel;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                _efLog( new LogOutput(logLevel, eventId, formatter(state, exception)));
                Console.WriteLine(formatter(state, exception));
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
}