// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;
using TestSupport.EfHelpers;
using TestSupport.EfHelpers.Internal;
using TestSupport.Helpers;

namespace Test.Helpers
{
    /// <summary>
    /// This static class contains extension methods to use with PostgreSql databases
    /// </summary>
    public static class PostgreSqlHelpers
    {
        /// <summary>
        /// This creates the DbContextOptions  options for a PostgreSql database, 
        /// where the database name is formed using the appsetting's PostgreSqlConnection with the class name as a prefix.
        /// That is, the database is unique to the object provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the test class you are in</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreatePostgreSqlUniqueDatabaseOptions<T>(this object callingClass, 
            Action<DbContextOptionsBuilder<T>> builder = null)
            where T : DbContext
        {
            return CreatePostgreSqlOptionWithDatabaseName(callingClass, null, builder).Options;
        }

        /// <summary>
        /// This creates the DbContextOptions options for a PostgreSql database while capturing EF Core's logging output. 
        /// The database name is formed using the appsetting's PostgreSqlConnection with the class name as a prefix.
        /// That is, the database is unique to the object provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="logAction">This action is called with each log output</param>
        /// <param name="logToOptions">Optional: This allows you to define what logs you want and what format. Defaults to LogLevel.Information</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreatePostgreSqlUniqueClassOptionsWithLogTo<T>(this object callingClass,
            Action<string> logAction,
            LogToOptions logToOptions = null, Action<DbContextOptionsBuilder<T>> builder = null)
            where T : DbContext
        {
            if (logAction == null) throw new ArgumentNullException(nameof(logAction));

            return CreatePostgreSqlOptionWithDatabaseName<T>(callingClass, null, builder)
                    .AddLogTo(logAction, logToOptions)
                    .Options;
        }

        /// <summary>
        /// This creates the DbContextOptions options for a PostgreSql database, 
        /// where the database name is formed using the appsetting's PostgreSqlConnection 
        /// with the class name and the calling method's name as as a prefix.
        /// That is, the database is unique to the calling method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <param name="callingMember">Do not use: this is filled in by compiler</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreatePostgreSqlUniqueMethodOptions<T>(this object callingClass,
            Action<DbContextOptionsBuilder<T>> builder = null,
            [CallerMemberName] string callingMember = "") where T : DbContext
        {
            return CreatePostgreSqlOptionWithDatabaseName<T>(callingClass, callingMember, builder).Options;
        }

        //------------------------------------------------------------
        //methods to provide empty PostgreSql databases and how to delete all the PostgreSql databases used for unit testing

        static Checkpoint EmptyCheckpoint = new Checkpoint
        {
            DbAdapter = DbAdapter.Postgres
        };

        /// <summary>
        /// This will ensure that there is a PostgreSql database, and that database has no rows in any tables
        /// NOTE: If you change anything that alters the database schema, then you must delete the database 
        /// and have EF Core recreate the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public async static Task EnsureCreatedAndEmptyPostgreSqlAsync<T>(this T context)
            where T : DbContext
        {
            if(!context.Database.EnsureCreated())
            {
                //Already created, so wipe it using respwan
                var connectionString = context.Database.GetConnectionString();
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    await EmptyCheckpoint.Reset(conn);
                }
            };
        }

        /// <summary>
        /// This will delete all PostgreSql databases that start with the database name in the default connection string
        /// WARNING: This will delete multiple databases - make sure your <see cref="AppSettings.PostgreSqlConnectionString"/> database name is unique!!!
        /// </summary>
        /// <returns>Number of databases deleted</returns>
        public static int DeleteAllPostgreSqlUnitTestDatabases()
        {
            var config = AppSettings.GetConfiguration(Assembly.GetCallingAssembly());
            var baseConnection = config.GetConnectionString(AppSettings.PostgreSqlConnectionString);
            if (string.IsNullOrEmpty(baseConnection))
                throw new InvalidOperationException(
                    $"Your {AppSettings.AppSettingFilename} file isn't set up for the '{AppSettings.PostgreSqlConnectionString}'.");

            var databaseNamesToDelete = baseConnection.GetAllPostgreUnitTestDatabases();

            var builder = new NpgsqlConnectionStringBuilder(baseConnection);
            builder.Database = "postgres";
            foreach (var databaseName in databaseNamesToDelete)
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(builder.ToString()))
                {
                    void ExecuteScalar(string cmdText)
                    {
                        using (NpgsqlCommand cmd = new NpgsqlCommand(cmdText, conn))
                        {
                            var result = cmd.ExecuteScalar();
                        }
                    }

                    conn.Open();
                    //The following commands were taken from EF Core
                    //also see https://www.postgresqltutorial.com/postgresql-drop-database/ for another form
                    ExecuteScalar($"REVOKE CONNECT ON DATABASE \"{databaseName}\" FROM PUBLIC");                  
                    ExecuteScalar("SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity " +
                        $"WHERE datname = '{databaseName}'");
                    ExecuteScalar($"DROP DATABASE \"{databaseName}\"");
                }
            }
            return databaseNamesToDelete.Count;
        }

        //------------------------------------
        //private methods

        private static DbContextOptionsBuilder<T> CreatePostgreSqlOptionWithDatabaseName<T>(object callingClass,
            string callingMember, Action<DbContextOptionsBuilder<T>> extraOptions)
            where T : DbContext
        {
            var connectionString = callingClass.GetUniquePostgreSqlConnectionString(callingMember);
            var builder = new DbContextOptionsBuilder<T>();
            builder.UseNpgsql(connectionString);
            builder.ApplyOtherOptionSettings();
            extraOptions?.Invoke(builder);

            return builder;
        }

        private static List<string> GetAllPostgreUnitTestDatabases(this string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var orgDbStartsWith = builder.Database;
            builder.Database = "postgres";
            var newConnectionString = builder.ToString();

            var result = new List<string>();
            using (NpgsqlConnection conn = new NpgsqlConnection(newConnectionString))
            {
                conn.Open();
                string cmdText = $"SELECT datName FROM pg_database WHERE datname LIKE '{orgDbStartsWith}%'";
                using (NpgsqlCommand cmd = new NpgsqlCommand(cmdText, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return result;
        }
    }
}