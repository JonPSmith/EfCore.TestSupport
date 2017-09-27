// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TestSupport.Helpers;

namespace TestSupport.EfHelpers
{
    public static class SqlAdoNetHelpers
    {
        //Note that there is no initial catalog
        private const string LocalHostConnectionString = "Server=(localdb)\\mssqllocaldb;Initial Catalog=;Trusted_Connection=True;";

        /// <summary>
        /// This will delete all the databases that start with the database name in the default connection string
        /// WARNING: This will delete multiple databases - make sure your DefaultConnection database name is unique!!!
        /// </summary>
        /// <returns>Number of databases deleted</returns>
        public static int DeleteAllUnitTestDatabases()
        {
            var config = AppSettings.GetConfiguration();
            var builder = new SqlConnectionStringBuilder(config.GetConnectionString(AppSettings.UnitTestConnectionStringName));
            var orgDbName = builder.InitialCatalog;

            var databaseNamesToDelete = GetAllMatchingDatabases(orgDbName);
            foreach (var databaseName in databaseNamesToDelete)
            {
                databaseName.DeleteDatabase();
            }
            return databaseNamesToDelete.Count;
        }

        public static List<string> GetAllMatchingDatabases(this string startsWith,
            string connectionString = LocalHostConnectionString)
        {
            var result = new List<string>();

            using (var myConn = new SqlConnection(connectionString))
            {
                var command = $"SELECT name FROM master.dbo.sysdatabases WHERE name LIKE '{startsWith}%'";
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

        public static int ExecuteRowCount(this string connectionString, string tableName, string whereClause = "")
        {
            using (var myConn = new SqlConnection(connectionString))
            {
                var command = "SELECT COUNT(*) FROM " + tableName + " " + whereClause;
                var myCommand = new SqlCommand(command, myConn);
                myConn.Open();
                return (int) myCommand.ExecuteScalar();
            }
        }

        public static int ExecuteNonQuery(this string connection, string command, int commandTimeout = 10)
        {
            using (var myConn = new SqlConnection(connection))
            {
                var myCommand = new SqlCommand(command, myConn) {CommandTimeout = commandTimeout};
                myConn.Open();
                return myCommand.ExecuteNonQuery();
            }
        }
    }
}