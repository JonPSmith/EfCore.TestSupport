// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using TestSupport.EfHelpers.Internal;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// This holds logs produced by the MyLoggerProvider 
    /// </summary>
    public class LogOutput 
    {
        private const string EfCoreEventIdStartWith = "Microsoft.EntityFrameworkCore";

        /// <summary>
        /// The logLevel of this log
        /// </summary>
        public LogLevel LogLevel { get; } 

        /// <summary>
        /// The logging EventId - should be string for EF Core logs
        /// </summary>
        public EventId EventId { get; }  

        /// <summary>
        /// The message in the log
        /// </summary>
        public string Message { get; }  

        /// <summary>
        /// This returns the last part of an EF Core EventId name, or null if the eventId is not an EF Core one
        /// </summary>
        private string EfEventIdLastName => 
            EventId.Name?.StartsWith(                
                     EfCoreEventIdStartWith) == true 
                ? EventId.Name.Split('.').Last()     
                : null;                              

        internal LogOutput(LogLevel logLevel, 
            EventId eventId, string message) 
        {                                    
            LogLevel = logLevel;             
            EventId = eventId;               
            Message = message;               
        }                                    

        /// <summary>
        /// Summary of the log
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var logType = EfEventIdLastName == null ? "" : "," + EfEventIdLastName;        
            return $"{LogLevel}{logType}: {Message}";
        }

        /// <summary>
        /// This tries to build valid SQL commands on CommandExecuted logs, i.e. logs containing the SQL output
        /// by taking the values available from EnableSensitiveDataLogging and inserting them in place of the parameter.
        /// This makes it easier to copy the SQL produced by EF Core and run in SSMS etc.
        /// LIMITATIONS are:
        /// - It can't distinguish the different between an empty string and a null string - it default to null
        /// - It can't work out if its a byte[] or not, so byte[] is treated as a SQL string, WHICH WILL fail
        /// - Numbers are presented as SQL strings, e.g. 123 becomes '123'. SQL Server can handle that
        /// </summary>
        /// <param name="sensitiveLoggingEnabled"></param>
        /// <returns></returns>
        public string DecodeMessage(bool sensitiveLoggingEnabled = true)
        {
            if (!sensitiveLoggingEnabled)
                return Message;

            return EfCoreLogDecoder.DecodeMessage(this);
        }

    }
}