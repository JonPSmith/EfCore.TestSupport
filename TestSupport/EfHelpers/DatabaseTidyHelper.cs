// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TestSupport.Helpers;

namespace TestSupport.EfHelpers
{
    public static class DatabaseTidyHelper
    {
        //Note that there is no initial catalog
        internal const string LocalHostConnectionString = "Server=(localdb)\\mssqllocaldb;Initial Catalog=;Trusted_Connection=True;";

        internal static void DeleteDatabase(this string databaseName)
        {
            if (LocalHostConnectionString.ExecuteRowCount("sys.databases", $"WHERE [Name] = '{databaseName}'") == 1)
                LocalHostConnectionString.ExecuteNonQuery("DROP DATABASE [" + databaseName + "]");
            if (LocalHostConnectionString.ExecuteRowCount("sys.databases", $"WHERE [Name] = '{databaseName}'") == 1)
                //it failed
                throw new InvalidOperationException($"Failed to deleted {databaseName}. Did you have SSMS open or something?");
        }

        public static int DeleteAllUnitTestDatabases()
        {
            var config = AppSettings.GetConfiguration();
            var builder = new SqlConnectionStringBuilder(config.GetConnectionString(AppSettings.UnitTestConnectionStringName));
            var orgDbName = builder.InitialCatalog;

            var databaseNamesToDelete = GetAllMatchingDatabases($"{orgDbName}.");
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
    }
}