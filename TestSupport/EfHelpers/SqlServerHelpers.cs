// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using TestSupport.Helpers;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// This static class contains extension methods to use with SQL Server databases
    /// </summary>
    public static class SqlServerHelpers
    {
#if NETSTANDARD2_0
        /// <summary>
        /// This creates the DbContextOptions  options for a SQL server database, 
        /// where the database name is formed using the appsetting's DefaultConnection with the class name as a prefix.
        /// That is, the database is unique to the object provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="throwOnClientServerWarning">Optional: default will throw exception if QueryClientEvaluationWarning is logged. Set to false if not needed</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueClassOptions<T>(this object callingClass, bool throwOnClientServerWarning = true) 
            where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, throwOnClientServerWarning).Options;
        }
#elif NETSTANDARD2_1
        /// <summary>
        /// This creates the DbContextOptions  options for a SQL server database, 
        /// where the database name is formed using the appsetting's DefaultConnection with the class name as a prefix.
        /// That is, the database is unique to the object provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="applyExtraOption">Optional: action that allows you to add extra options to the builder</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueClassOptions<T>(this object callingClass, Action<DbContextOptionsBuilder<T>> applyExtraOption = null)
            where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, null, applyExtraOption).Options;
        }
#endif

#if NETSTANDARD2_0
        /// <summary>
        /// This creates the DbContextOptions options for a SQL server database while capturing EF Core's logging output. 
        /// The database name is formed using the appsetting's DefaultConnection with the class name as a prefix.
        /// That is, the database is unique to the object provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="efLog">This is a method that receives a LogOutput whenever EF Core logs something</param>
        /// <param name="logLevel">Optional: Sets the logLevel you want to capture. Defaults to Information</param>
        /// <param name="throwOnClientServerWarning">Optional: default will throw exception if QueryClientEvaluationWarning is logged. Set to false if not needed</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueClassOptionsWithLogging<T>(this object callingClass, Action<LogOutput> efLog, LogLevel logLevel = LogLevel.Information, bool throwOnClientServerWarning = true) 
            where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, throwOnClientServerWarning)
                .UseLoggerFactory(new LoggerFactory(new[] { new MyLoggerProviderActionOut(efLog, logLevel) }))
                .Options;
        }
#elif NETSTANDARD2_1
        /// <summary>
        /// This creates the DbContextOptions options for a SQL server database while capturing EF Core's logging output. 
        /// The database name is formed using the appsetting's DefaultConnection with the class name as a prefix.
        /// That is, the database is unique to the object provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="efLog">This is a method that receives a LogOutput whenever EF Core logs something</param>
        /// <param name="logLevel">Optional: Sets the logLevel you want to capture. Defaults to Information</param>
        /// <param name="applyExtraOption">Optional: action that allows you to add extra options to the builder</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueClassOptionsWithLogging<T>(this object callingClass,
            Action<LogOutput> efLog, LogLevel logLevel = LogLevel.Information, 
            Action<DbContextOptionsBuilder<T>> applyExtraOption = null)
            where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, null, applyExtraOption)
                .UseLoggerFactory(new LoggerFactory(new[] { new MyLoggerProviderActionOut(efLog, logLevel) }))
                .Options;
        }
#endif

#if NETSTANDARD2_0
        /// <summary>
        /// This creates the DbContextOptions options for a SQL server database, 
        /// where the database name is formed using the appsetting's DefaultConnection with the class name and the calling method's name as as a prefix.
        /// That is, the database is unique to the calling method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="throwOnClientServerWarning">Optional: default will throw exception if QueryClientEvaluationWarning is logged. Set to false if not needed</param>
        /// <param name="callingMember">Do not use: this is filled in by compiler</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueMethodOptions<T>(this object callingClass, bool throwOnClientServerWarning = true,
            [CallerMemberName] string callingMember = "") where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, throwOnClientServerWarning, callingMember).Options;
        }
#elif NETSTANDARD2_1
        /// <summary>
        /// This creates the DbContextOptions options for a SQL server database, 
        /// where the database name is formed using the appsetting's DefaultConnection with the class name and the calling method's name as as a prefix.
        /// That is, the database is unique to the calling method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="applyExtraOption">Optional: action that allows you to add extra options to the builder</param>
        /// <param name="callingMember">Do not use: this is filled in by compiler</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueMethodOptions<T>(this object callingClass,
            Action<DbContextOptionsBuilder<T>> applyExtraOption = null,
            [CallerMemberName] string callingMember = "") where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, callingMember, applyExtraOption).Options;
        }
#endif

#if NETSTANDARD2_0
        /// <summary>
        /// This creates the DbContextOptions  options for a SQL server database while capturing EF Core's logging output. 
        /// Where the database name is formed using the appsetting's DefaultConnection with the class name and the calling method's name as as a prefix.
        /// That is, the database is unique to the calling method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="efLog">This is a method that receives a LogOutput whenever EF Core logs something</param>
        /// <param name="logLevel">Optional: Sets the logLevel you want to capture. Defaults to Information</param>
        /// <param name="throwOnClientServerWarning">Optional: default will throw exception if QueryClientEvaluationWarning is logged. Set to false if not needed</param>
        /// <param name="callingMember">Do not use: this is filled in by compiler</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueMethodOptionsWithLogging<T>(this object callingClass, Action<LogOutput> efLog, 
            LogLevel logLevel = LogLevel.Information, bool throwOnClientServerWarning = true,
            [CallerMemberName] string callingMember = "") where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, throwOnClientServerWarning, callingMember)
                .UseLoggerFactory(new LoggerFactory(new[] { new MyLoggerProviderActionOut(efLog, logLevel) }))
                .Options;
        }
#elif NETSTANDARD2_1
        /// <summary>
        /// This creates the DbContextOptions  options for a SQL server database while capturing EF Core's logging output. 
        /// Where the database name is formed using the appsetting's DefaultConnection with the class name and the calling method's name as as a prefix.
        /// That is, the database is unique to the calling method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="efLog">This is a method that receives a LogOutput whenever EF Core logs something</param>
        /// <param name="logLevel">Optional: Sets the logLevel you want to capture. Defaults to Information</param>
        /// <param name="applyExtraOption">Optional: action that allows you to add extra options to the builder</param>
        /// <param name="callingMember">Do not use: this is filled in by compiler</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueMethodOptionsWithLogging<T>(this object callingClass,
            Action<LogOutput> efLog,
            LogLevel logLevel = LogLevel.Information,
            Action<DbContextOptionsBuilder<T>> applyExtraOption = null,
            [CallerMemberName] string callingMember = "") where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, callingMember,applyExtraOption)
                .UseLoggerFactory(new LoggerFactory(new[] { new MyLoggerProviderActionOut(efLog, logLevel) }))
                .Options;
        }
#endif

        /// <summary>
        /// This will ensure an empty database by deleting the current database and recreating it
        /// </summary>
        /// <param name="context"></param>
        public static void CreateEmptyViaDelete(this DbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        /// <summary>
        /// This will ensure an empty database by using the WipeAllDataFromDatabase method
        /// </summary>
        /// <param name="context"></param>
        /// <param name="addBracketsAndSchema">Optional: normally it only uses the table name, but for cases where you have multiple schemas,
        /// or a table name that needs brackets the you can set to to true. Default is false</param>
        /// <param name="maxDepth">Valuse to stop the wipe method from getting in a circular reference loop</param>
        /// <param name="excludeTypes">This allows you to provide the Types of the table that you don't want wiped. 
        /// Useful if you have a circular ref that WipeAllDataFromDatabase cannot handle. You then must wipe that part.</param>
        /// <returns>True if the database is created, false if it already existed.</returns>
        public static bool CreateEmptyViaWipe(this DbContext context,
            bool addBracketsAndSchema = false,
            int maxDepth = 10, params Type[] excludeTypes)
        {
            var databaseWasCreated = context.Database.EnsureCreated();
            if (!databaseWasCreated)
                context.WipeAllDataFromDatabase(addBracketsAndSchema, maxDepth, excludeTypes);
            return databaseWasCreated;
        }

        //------------------------------------
        //private methods

#if NETSTANDARD2_0
        private static DbContextOptionsBuilder<T> CreateOptionWithDatabaseName<T>(object callingClass, bool throwOnClientServerWarning, 
                string callingMember = null)    
            where T : DbContext
        {
            var connectionString = callingClass.GetUniqueDatabaseConnectionString( callingMember);                 
            var builder = new DbContextOptionsBuilder<T>();  
            builder.UseSqlServer(connectionString);
            builder.ApplyOtherOptionSettings(throwOnClientServerWarning);       

            return builder; 
        }
#elif NETSTANDARD2_1
        private static DbContextOptionsBuilder<T> CreateOptionWithDatabaseName<T>(object callingClass,
            string callingMember, Action<DbContextOptionsBuilder<T>> applyExtraOption)
            where T : DbContext
        {
            var connectionString = callingClass.GetUniqueDatabaseConnectionString(callingMember);
            var builder = new DbContextOptionsBuilder<T>();
            builder.UseSqlServer(connectionString);
            builder.ApplyOtherOptionSettings();
            applyExtraOption?.Invoke(builder);

            return builder;
        }
#endif

#if NETSTANDARD2_0
        internal static void ApplyOtherOptionSettings<T>(this DbContextOptionsBuilder<T> builder, bool throwOnClientServerWarning) 
            where T : DbContext
        {
            builder.EnableSensitiveDataLogging();
            if (throwOnClientServerWarning)
            {
                builder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
            }
        }
#elif NETSTANDARD2_1
        internal static void ApplyOtherOptionSettings<T>(this DbContextOptionsBuilder<T> builder)
            where T : DbContext
        {
            builder.EnableSensitiveDataLogging();
        }
#endif
    }
}