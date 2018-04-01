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
        /// This returns all the matching databases that start with the startsWith parameter 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        private static List<string> GetAllMatchingDatabases(string orgDbName, string connectionString)
        {
            var result = new List<string>();


            using (var myConn = new SqlConnection(connectionString))
            {
                var command = $"SELECT name FROM master.dbo.sysdatabases WHERE name LIKE '{orgDbName}%'";
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