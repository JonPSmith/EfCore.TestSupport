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
        /// <param name="throwOnClientServerWarning">Optional: if set to true then will throw exception if QueryClientEvaluationWarning is logged</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateOptions<T>(bool throwOnClientServerWarning = false) where T : DbContext
        {
            //Thanks to https://www.scottbrady91.com/Entity-Framework/Entity-Framework-Core-In-Memory-Testing
            var connectionStringBuilder =
                new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);
            connection.Open();                //see https://github.com/aspnet/EntityFramework/issues/6968

            // create in-memory context
            var builder = new DbContextOptionsBuilder<T>();
            builder.UseSqlite(connection);
            builder.ApplyOtherOptionSettings(throwOnClientServerWarning);

            return builder.Options;
        }

        internal static void ApplyOtherOptionSettings<T>(this DbContextOptionsBuilder<T> builder, bool throwOnClientServerWarning) where T : DbContext
        {
            builder.EnableSensitiveDataLogging();  //You get more information with this turned on.
            if (throwOnClientServerWarning)
            {
                //This will throw an exception of a QueryClientEvaluationWarning is logged
                builder.ConfigureWarnings(warnings =>
                    warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
            }
        }
    }
}