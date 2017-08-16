// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace TestSupport.EfHelpers
{
    public static class ScriptMigrateHelpers
    {

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
                databaseName.DeleteDatabase();
            }
            nonDbConnectionString.ExecuteNonQuery("CREATE DATABASE [" + databaseName + "]", timeout);
        }

        /// <summary>
        /// This reads in a SQL script file and executes each command within a transaction
        /// Each command should have an GO at the start of the line after the command.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="filePath"></param>
        public static void ExecuteScriptFileInTransaction(this string connectionString, string filePath)
        {
            var scriptContent = File.ReadAllText(filePath);
            Regex regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string[] lines = regex.Split(scriptContent);

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                SqlTransaction transaction = sqlConnection.BeginTransaction();
                using (SqlCommand cmd = sqlConnection.CreateCommand())
                {
                    cmd.Connection = sqlConnection;
                    cmd.Transaction = transaction;

                    foreach (string line in lines)
                    {
                        if (line.Length > 0)
                        {
                            cmd.CommandText = line;
                            cmd.CommandType = CommandType.Text;

                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (SqlException)
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }
                    }
                }
                transaction.Commit();
            }
        }
    }
}