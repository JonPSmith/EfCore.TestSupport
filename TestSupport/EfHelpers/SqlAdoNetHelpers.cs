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