// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Test.EfHelpers
{
    public class MyLoggerProvider : ILoggerProvider //#A
    {
        private readonly List<string> _logs; //#B

        public MyLoggerProvider(List<string> logs) //#B
        {
            _logs = logs;
        }

        public ILogger CreateLogger(string categoryName) //#C
        {
            return new MyLogger(_logs);
        }

        public void Dispose()
        { }

        private class MyLogger : ILogger //#D
        {
            private readonly List<string> _logs; //#E

            public MyLogger(List<string> logs) //#E
            {
                _logs = logs;
            }

            public bool IsEnabled(LogLevel logLevel) //#F
            {
                return logLevel >= LogLevel.Information;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, //#G
                TState state, Exception exception, 
                Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel)) //#H
                    return;

                //_logs.Add(formatter(state, exception)); //#I
                _logs.Add($"{logLevel}: "+ //#J
                    formatter(state, exception) + //#J
                    (exception == null            //#J
                       ? ""                       //#J
                       : ", Exception = \n" + exception));//#J
                Console.WriteLine(formatter(state, exception));//#K
            }

            public IDisposable BeginScope<TState>(TState state) //#L
            {
                return null;
            }
        }
    }
    /******************************************************************
    #A I need to provide a ILoggerProvider, which has a method to create a logger
    #B This logger logs to a List<string> so we need to hold a reference to it
    #C This method is called whenever a new logger is created. The category name is the name of the logger, which you could use to select different loggers
    #D This in my logger, which conforms to the ILogger interface
    #E I pass in the List<string> that my logger will log to
    #F This defines what the logger should log based on the LogLevel. Useful for filtering out unnecessary logging messages
    #G This is the inner method that is called for all logging. Normally the developer will call something like LogInformation, which then calls this method
    #H We exit if the logging is not enabled for this level
    #I This is the standard Microsoft format a log, but leaves out the logLevel and any exception message. see  https://github.com/aspnet/Logging/issues/442
    #J Here is my formatted version, with the logLevel and the exception message if present
    #K I also write to the console. When using Resharper to run the unit test in debug mode you get a window, so you can see the console output
    #L This is for scoped logging. You can find out about this at https://msdn.microsoft.com/en-us/magazine/mt694089.aspx
     * ****************************************************************/
}