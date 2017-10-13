// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TestSupport.EfHelpers
{
    public class LogOutput //#A
    {
        private const string EfCoreEventIdStartWith //#B
            = "Microsoft.EntityFrameworkCore";      //#B

        public LogLevel LogLevel { get; } //#C
        public EventId EventId { get; }  //#D
        public string Message { get; }  //#E

        /// <summary>
        /// This returns the last part of an EF Core EventId name, or null if the eventId is not an EF Core one
        /// </summary>
        public string EfEventIdLastName => //#F
            EventId.Name?.StartsWith(                //#G
                     EfCoreEventIdStartWith) == true //#G
                ? EventId.Name.Split('.').Last()     //#G
                : null;                              //#G

        internal LogOutput(LogLevel logLevel,//#H 
            EventId eventId, string message) //#H
        {                                    //#H
            LogLevel = logLevel;             //#H
            EventId = eventId;               //#H
            Message = message;               //#H
        }                                    //#H

        public override string ToString() //#G
        {
            return 
                $"{LogLevel},{EfEventIdLastName}: " +
                Message;
        }
    }
    /***************************************************************
    #A This class holds each log captured from EF Core
    #B I use this string to identify logs that were produced by EF Core
    #C This holds what LogLevel the log was reported at, for istance, Information, Warning, Error
    #D This holds the EventId - useful because EF Core 2.0.0 has named events
    #E This is the logged message
    #F This property returns the last part of the name, but only if its an EF Core log. Useful, as it is a quick way to identify specific events
    #G This gets either the last part of the EF Core eventid name, or null if not EF Core
    #H The constructor for the class
    #I Typically you will show the logs as text, so the ToString method returns a useful string
     * **************************************************************/
}