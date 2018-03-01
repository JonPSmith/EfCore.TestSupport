// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
        public static DbContextOptions<T> CreateOptions<T>
            (bool throwOnClientServerWarning = true) //#A
            where T : DbContext
        {
            //Thanks to https://www.scottbrady91.com/Entity-Framework/Entity-Framework-Core-In-Memory-Testing
            var connectionStringBuilder =         //#B
                new SqliteConnectionStringBuilder //#B
                    { DataSource = ":memory:" };  //#B
            var connectionString =                  //#C
                connectionStringBuilder.ToString(); //#C
            var connection =                            //#D
                new SqliteConnection(connectionString); //#D
            connection.Open();  //#E             //see https://github.com/aspnet/EntityFramework/issues/6968

            // create in-memory context
            var builder = 
                new DbContextOptionsBuilder<T>();
            builder.UseSqlite(connection); //#F
            builder.ApplyOtherOptionSettings  //#G
                (throwOnClientServerWarning); //#G

            return builder.Options; //#H
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

        #F I return the DbContextOptions<T> to use in the creation of my application's DbContext
         * **************************************************************/
    }
}