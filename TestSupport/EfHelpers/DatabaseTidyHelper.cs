// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TestSupport.Helpers;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// Extension methods for deleting all the databases used in the unit test
    /// </summary>
    public static class DatabaseTidyHelper
    {
        /// <summary>
        /// This will delete all the databases that start with the database name in the default connection string
        /// WARNING: This will delete multiple databases - make sure your DefaultConnection database name is unique!!!
        /// </summary>
        /// <returns>Number of databases deleted</returns>
        public static int DeleteAllSqlServerTestDatabases()
        {
            var config = AppSettings.GetConfiguration(Assembly.GetCallingAssembly());
            var builder = new SqlConnectionStringBuilder(config.GetConnectionString(AppSettings.UnitTestConnectionStringName));
            var orgDbName = builder.InitialCatalog;
            builder.InitialCatalog = "";

            var databaseNamesToDelete = GetAllMatchingDatabases(orgDbName, builder.ToString());
            foreach (var databaseName in databaseNamesToDelete)
            {
                databaseName.DeleteDatabase(builder.ToString());
            }
            return databaseNamesToDelete.Count;
        }

        /// <summary>
        /// This returns all the matching databases that start with the orgDbStartsWith parameter 
        /// </summary>
        /// <param name="orgDbStartsWith">Start of database name</param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private static List<string> GetAllMatchingDatabases(string orgDbStartsWith, string connectionString)
        {
            var result = new List<string>();


            using (var myConn = new SqlConnection(connectionString))
            {
                var command = $"SELECT name FROM master.dbo.sysdatabases WHERE name LIKE '{orgDbStartsWith}%'";
                var myCommand = new SqlCommand(command, myConn);
                myConn.Open();
                using (var reader = myCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Wipes out the existing database and creates a new, empty one
        /// </summary>
        /// <param name="databaseConnectionString">a actual connection string</param>
        /// <param name="timeout">Defines a timeout for connection and the SQL DELETE/CREATE database commands</param>
        public static void WipeCreateDatabase(this string databaseConnectionString, int timeout = 30)
        {
            var builder = new SqlConnectionStringBuilder(databaseConnectionString);
            var databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "";          //remove database, as create database won't work with it
            builder.ConnectTimeout = timeout;

            var nonDbConnectionString = builder.ToString();
            if (nonDbConnectionString.ExecuteRowCount("sys.databases", string.Format("WHERE [Name] = '{0}'", databaseName)) == 1)
            {
                databaseName.DeleteDatabase(nonDbConnectionString);
            }
            nonDbConnectionString.ExecuteNonQuery("CREATE DATABASE [" + databaseName + "]", timeout);
        }

        private static void DeleteDatabase(this string databaseName, string connectionString)
        {
            if (connectionString.ExecuteRowCount("sys.databases", $"WHERE [Name] = '{databaseName}'") == 1)
                connectionString.ExecuteNonQuery("DROP DATABASE [" + databaseName + "]");
            if (connectionString.ExecuteRowCount("sys.databases", $"WHERE [Name] = '{databaseName}'") == 1)
                //it failed
                throw new InvalidOperationException($"Failed to deleted {databaseName}. Did you have SSMS open or something?");
        }


        //---------------------------------------------------------------------------
        //PostgreSQL version

        /// <summary>
        /// This will delete all PostgreSql databases that start with the database name in the default connection string
        /// WARNING: This will delete multiple databases - make sure your <see cref="AppSettings.PostgreSqlConnectionString"/> database name is unique!!!
        /// </summary>
        /// <returns>Number of databases deleted</returns>
        public static int DeleteAllPostgreSqlTestDatabases()
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
                builder.ToString().ExecuteScalars(
                    $"REVOKE CONNECT ON DATABASE \"{databaseName}\" FROM PUBLIC",
                    "SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity " +
                        $"WHERE datname = '{databaseName}'",
                    $"DROP DATABASE \"{databaseName}\""
                );
            }
            return databaseNamesToDelete.Count;
        }

        //------------------------------------
        //private methods

        private static void ExecuteScalars(this string connectionString, params string[] commands)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                foreach (var commandText in commands)
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                    {
                        var result = cmd.ExecuteScalar();
                    }
                }
            }
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