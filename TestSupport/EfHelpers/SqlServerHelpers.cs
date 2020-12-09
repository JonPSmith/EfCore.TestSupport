// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestSupport.EfHelpers.Internal;
using TestSupport.Helpers;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// This static class contains extension methods to use with SQL Server databases
    /// </summary>
    public static class SqlServerHelpers
    {
        /// <summary>
        /// This creates the DbContextOptions  options for a SQL server database, 
        /// where the database name is formed using the appsetting's DefaultConnection with the class name as a prefix.
        /// That is, the database is unique to the object provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueClassOptions<T>(this object callingClass, Action<DbContextOptionsBuilder<T>> builder = null)
            where T : DbContext
        {
            return CreateOptionWithDatabaseName(callingClass, null, builder).Options;
        }

        /// <summary>
        /// This creates the DbContextOptions options for a SQL server database while capturing EF Core's logging output. 
        /// The database name is formed using the appsetting's DefaultConnection with the class name as a prefix.
        /// That is, the database is unique to the object provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="efLog">This is a method that receives a LogOutput whenever EF Core logs something</param>
        /// <param name="logLevel">Optional: Sets the logLevel you want to capture. Defaults to Information</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <returns></returns>
        [Obsolete("Suggest using CreateUniqueClassOptionsWithLogTo<T> which gives more logging options")]
        public static DbContextOptions<T> CreateUniqueClassOptionsWithLogging<T>(this object callingClass,
            Action<LogOutput> efLog, LogLevel logLevel = LogLevel.Information, 
            Action<DbContextOptionsBuilder<T>> builder = null)
            where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, null, builder)
                .UseLoggerFactory(new LoggerFactory(new[] { new MyLoggerProviderActionOut(efLog, logLevel) }))
                .Options;
        }

        /// <summary>
        /// This creates the DbContextOptions options for a SQL server database while capturing EF Core's logging output. 
        /// The database name is formed using the appsetting's DefaultConnection with the class name as a prefix.
        /// That is, the database is unique to the object provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="logAction">This action is called with each log output</param>
        /// <param name="logToOptions">Optional: This allows you to define what logs you want and what format. Defaults to LogLevel.Information</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueClassOptionsWithLogTo<T>(this object callingClass,
            Action<string> logAction,
            LogToOptions logToOptions = null, Action<DbContextOptionsBuilder<T>> builder = null)
            where T : DbContext
        {
            if (logAction == null) throw new ArgumentNullException(nameof(logAction));

            return CreateOptionWithDatabaseName<T>(callingClass, null, builder)
                    .AddLogTo(logAction, logToOptions)
                    .Options;
        }

        /// <summary>
        /// This creates the DbContextOptions options for a SQL server database, 
        /// where the database name is formed using the appsetting's DefaultConnection with the class name and the calling method's name as as a prefix.
        /// That is, the database is unique to the calling method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <param name="callingMember">Do not use: this is filled in by compiler</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueMethodOptions<T>(this object callingClass,
            Action<DbContextOptionsBuilder<T>> builder = null,
            [CallerMemberName] string callingMember = "") where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, callingMember, builder).Options;
        }

        /// <summary>
        /// This creates the DbContextOptions  options for a SQL server database while capturing EF Core's logging output. 
        /// Where the database name is formed using the appsetting's DefaultConnection with the class name and the calling method's name as as a prefix.
        /// That is, the database is unique to the calling method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="efLog">This is a method that receives a LogOutput whenever EF Core logs something</param>
        /// <param name="logLevel">Optional: Sets the logLevel you want to capture. Defaults to Information</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <param name="callingMember">Do not use: this is filled in by compiler</param>
        /// <returns></returns>
        [Obsolete("Suggest using CreateUniqueMethodOptionsWithLogTo<T> which gives more logging options")]
        public static DbContextOptions<T> CreateUniqueMethodOptionsWithLogging<T>(this object callingClass,
            Action<LogOutput> efLog,
            LogLevel logLevel = LogLevel.Information,
            Action<DbContextOptionsBuilder<T>> builder = null,
            [CallerMemberName] string callingMember = "") where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, callingMember,builder)
                .UseLoggerFactory(new LoggerFactory(new[] { new MyLoggerProviderActionOut(efLog, logLevel) }))
                .Options;
        }

        /// <summary>
        /// This creates the DbContextOptions  options for a SQL server database while capturing EF Core's logging output. 
        /// Where the database name is formed using the appsetting's DefaultConnection with the class name and the calling method's name as as a prefix.
        /// That is, the database is unique to the calling method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="logAction">This action is called with each log output</param>
        /// <param name="logToOptions">Optional: This allows you to define what logs you want and what format. Defaults to LogLevel.Information</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <param name="callingMember">Do not use: this is filled in by compiler</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueMethodOptionsWithLogTo<T>(this object callingClass,
                    Action<string> logAction,
                    LogToOptions logToOptions = null, Action<DbContextOptionsBuilder<T>> builder = null,
            [CallerMemberName] string callingMember = "") where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, callingMember,builder)
                .AddLogTo(logAction, logToOptions)
                .Options;
        }

        /// <summary>
        /// This will ensure an empty database by deleting the current database and recreating it
        /// </summary>
        /// <param name="context"></param>
        public static void CreateEmptyViaDelete(this DbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        //------------------------------------
        //private methods

        private static DbContextOptionsBuilder<T> CreateOptionWithDatabaseName<T>(object callingClass,
            string callingMember, Action<DbContextOptionsBuilder<T>> extraOptions)
            where T : DbContext
        {
            var connectionString = callingClass.GetUniqueDatabaseConnectionString(callingMember);
            var builder = new DbContextOptionsBuilder<T>();
            builder.UseSqlServer(connectionString);
            builder.ApplyOtherOptionSettings();
            extraOptions?.Invoke(builder);

            return builder;
        }
    }
}