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
            (bool throwOnClientServerWarning = true)
            where T : DbContext
        {
            //Thanks to https://www.scottbrady91.com/Entity-Framework/Entity-Framework-Core-In-Memory-Testing
            var connectionStringBuilder =         //#A
                new SqliteConnectionStringBuilder //#A
                    { DataSource = ":memory:" };  //#A
            var connectionString =                  //#B
                connectionStringBuilder.ToString(); //#B
            var connection =                            //#C
                new SqliteConnection(connectionString); //#C
            connection.Open();  //#D              //see https://github.com/aspnet/EntityFramework/issues/6968

            // create in-memory context
            var builder = 
                new DbContextOptionsBuilder<T>();
            builder.UseSqlite(connection); //E
            builder.ApplyOtherOptionSettings(throwOnClientServerWarning); //LEAVE OUT OF BOOK LISTING

            return builder.Options; //#F
        }
        /****************************************************************
        #A I need to create a SqLite connecton string with the DataSource set to ":memory:"
        #B I turn the SqliteConnectionStringBuilder into a string 
        #C And I form a Sqlite connection using the connection string
        #D I must open the Sqlite connection. If I don't then the in-memeory database doesn't work
        #E I now build a DbContextOptions<T> with the Sqlite database provider and the open connection
        #F I return the DbContextOptions<T> to use in the creation of my application's DbContext
         * **************************************************************/
    }
}