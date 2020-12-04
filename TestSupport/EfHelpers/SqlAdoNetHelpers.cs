// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Data.SqlClient;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// Contains extension methods to help with base SQL commands 
    /// </summary>
    public static class SqlAdoNetHelpers
    {
        /// <summary>
        /// Execute a count of the rows in a table, with optional where clause, using ADO.NET
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <param name="whereClause"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Execute a non-query SQL using ADO.NET
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(this string connectionString, string command, int commandTimeout = 10)
        {
            using (var myConn = new SqlConnection(connectionString))
            {
                var myCommand = new SqlCommand(command, myConn) {CommandTimeout = commandTimeout};
                myConn.Open();
                return myCommand.ExecuteNonQuery();
            }
        }
    }
}