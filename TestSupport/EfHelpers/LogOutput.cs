// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TestSupport.EfHelpers
{
    public class LogOutput
    {
        private const string EfCoreEventIdStartWith = "Microsoft.EntityFrameworkCore";

        public LogLevel LogLevel { get; private set; }
        public EventId EventId { get; private set; }
        public string Message { get; private set; }

        /// <summary>
        /// This returns the last part of an EF Core EventId name, or null if the eventId is not an EF Core one
        /// </summary>
        public string EfEventIdLastName => 
            EventId.Name?.StartsWith(EfCoreEventIdStartWith) == true
                ? EventId.Name.Split('.').Last()
                : null;

        public LogOutput(LogLevel logLevel, EventId eventId, string message)
        {
            LogLevel = logLevel;
            EventId = eventId;
            Message = message;
        }

        public override string ToString()
        {
            return $"{LogLevel},{EfEventIdLastName}: {Message}";
        }
    }
}