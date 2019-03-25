// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using Microsoft.Extensions.Configuration;
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
        public static int DeleteAllUnitTestDatabases()
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
            if (nonDbConnectionString.ExecuteRowCount("sys.databases", String.Format("WHERE [Name] = '{0}'", databaseName)) == 1)
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
    }
}