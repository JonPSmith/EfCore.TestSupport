// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestSupport.EfHelpers.Internal;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// This static class contains extension methods to use with in-memory Sqlite databases
    /// </summary>
    public static class SqliteInMemory
    {
        /// <summary>
        /// Created a Sqlite Options for in-memory database. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <returns></returns>
        public static DbContextOptionsDisposable<T> CreateOptions<T>(Action<DbContextOptionsBuilder<T>> builder = null)
            where T : DbContext
        {
            return new DbContextOptionsDisposable<T>(SetupConnectionAndBuilderOptions<T>(builder)
                .Options);
        }

        /// <summary>
        /// Created a Sqlite Options for in-memory database while using LogTo to get the EF Core logging output. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logAction">This action is called with each log output</param>
        /// <param name="logToOptions">Optional: This allows you to define what logs you want and what format. Defaults to LogLevel.Information</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <returns></returns>
        public static DbContextOptionsDisposable<T> CreateOptionsWithLogTo<T>(Action<string> logAction,
            LogToOptions logToOptions = null , Action<DbContextOptionsBuilder<T>> builder = null)
            where T : DbContext
        {
            if (logAction == null) throw new ArgumentNullException(nameof(logAction));

            return new DbContextOptionsDisposable<T>(
                SetupConnectionAndBuilderOptions(builder)
                    .AddLogTo(logAction, logToOptions)
                    .Options);
        }


        /// <summary>
        /// Created a Sqlite Options for in-memory database. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static DbContextOptionsBuilder<T> 
            SetupConnectionAndBuilderOptions<T> //#D
            (Action<DbContextOptionsBuilder<T>> applyExtraOption) //#E
            where T : DbContext
        {
            //Thanks to https://www.scottbrady91.com/Entity-Framework/Entity-Framework-Core-In-Memory-Testing
            var connectionStringBuilder =         //#F
                new SqliteConnectionStringBuilder //#F
                    { DataSource = ":memory:" };  //#F
            var connectionString = connectionStringBuilder.ToString(); //#G
            var connection = new SqliteConnection(connectionString); //#H
            connection.Open();  //#I             //see https://github.com/aspnet/EntityFramework/issues/6968

            // create in-memory context
            var builder = new DbContextOptionsBuilder<T>();
            builder.UseSqlite(connection); //#J
            builder.ApplyOtherOptionSettings(); //#K
            applyExtraOption?.Invoke(builder); //#L

            return builder; //#M
        }

        /****************************************************************
        #A A class containing the SQLite in-memory options which is also disposable
        #B This parameter allows you at add more option methods while building of the options
        #C Gets the DbContextOptions<T> and returns a disposable version
        #D This method builds the SQLite in-memory options
        #E This contains any extra option methods the user provided 
        #F Creates a SQLite connection string with the DataSource set to ":memory:"
        #G Turns the SQLiteConnectionStringBuilder into a connection string 
        #H Forms a SQLite connection using the connection string
        #I You must open the SQLite connection. If you don't, the in-memory database doesn't work.
        #J Builds a DbContextOptions<T> with the SQLite database provider and the open connection
        #K Calls a general method used on all your option builders. This enables sensitive logging and better error messages
        #L Add any extra options the user added
        #M Returns the DbContextOptions<T> to use in the creation of your application's DbContext
         * **************************************************************/
    }
}