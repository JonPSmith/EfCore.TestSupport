// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// This static class contains extention methods to use with in-memory Sqlite databases
    /// </summary>
    public static class SqliteInMemory
    {
        /// <summary>
        /// Created a Sqlite Options for in-memory database. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="throwOnClientServerWarning">Optional: default will throw exception if QueryClientEvaluationWarning is logged. Set to false if not needed</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateOptions<T> (bool throwOnClientServerWarning = true)
            where T : DbContext
        {
            return SetupConnectionAndBuilderOptions<T>(throwOnClientServerWarning).Options;
        }

        /// <summary>
        /// Created a Sqlite Options for in-memory database while capturing EF Core's logging output. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="efLog">This is a method that receives a LogOutput whenever EF Core logs something</param>
        /// <param name="throwOnClientServerWarning">Optional: default will throw exception if QueryClientEvaluationWarning is logged. Set to false if not needed</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateOptionsWithLogging<T>(Action<LogOutput> efLog, bool throwOnClientServerWarning = true)
            where T : DbContext
        {
            return SetupConnectionAndBuilderOptions<T>(throwOnClientServerWarning)
                .UseLoggerFactory(new LoggerFactory(new[] { new MyLoggerProviderActionOut(efLog)}))
                .Options;
        }


        /// <summary>
        /// Created a Sqlite Options for in-memory database. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="throwOnClientServerWarning">Optional: default will throw exception if QueryClientEvaluationWarning is logged. Set to false if not needed</param>
        /// <returns></returns>
        private static DbContextOptionsBuilder<T> SetupConnectionAndBuilderOptions<T>(bool throwOnClientServerWarning) //#A
            where T : DbContext
        {
            //Thanks to https://www.scottbrady91.com/Entity-Framework/Entity-Framework-Core-In-Memory-Testing
            var connectionStringBuilder =         //#B
                new SqliteConnectionStringBuilder //#B
                { DataSource = ":memory:" };  //#B
            var connectionString = connectionStringBuilder.ToString(); //#C
            var connection = new SqliteConnection(connectionString); //#D
            connection.Open();  //#E             //see https://github.com/aspnet/EntityFramework/issues/6968

            // create in-memory context
            var builder = new DbContextOptionsBuilder<T>();
            builder.UseSqlite(connection); //#F
            builder.ApplyOtherOptionSettings  //#G
                (throwOnClientServerWarning); //#G

            return builder; //#H
        }
        /****************************************************************
        #A By default it will throw an exception if a QueryClientEvaluationWarning is logged (see section 15.8). You can turn this off by providing a value of false as a parameter
        #B Creates a SQLite connection string with the DataSource set to ":memory:"
        #C Turns the SQLiteConnectionStringBuilder into a string 
        #D Forms a SQLite connection using the connection string
        #E You must open the SQLite connection. If you don't, the in-memory database doesn't work.
        #F Builds a DbContextOptions<T> with the SQLite database provider and the open connection
        #G Calls a general method used on all your option builders. This enables sensitive logging so you get more information, and if throwOnClientServerWarning is true, it configures the warning to throw on a QueryClientEvaluationWarning being logged
        #H Returns the DbContextOptions<T> to use in the creation of your application's DbContext
         * **************************************************************/
    }
}