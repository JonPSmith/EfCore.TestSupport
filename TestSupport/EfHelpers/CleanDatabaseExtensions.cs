// Copied from SqlServerDatabaseFacadeExtensions class and altered by GitHun @JonPSmith
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using TestSupport.EfHelpers.Internal;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// Static class holding the EnsureClean extension method
    /// </summary>
    public static class CleanDatabaseExtensions
    {
        /// <summary>
        /// Calling this will wipe the database schema down to nothing in the database and then calls
        /// Database.EnsureCreated to create the correct database schema based on your EF Core Context
        /// NOTE: This only works for SQL Server
        /// </summary>>
        /// <param name="databaseFacade">The Database property of the current DbContext that you want to clean</param>
        /// <param name="setUpSchema">Optional: by default it will set the schema to match the current DbContext configuration. If false leaves the database empty</param>
        public static void EnsureClean(this DatabaseFacade databaseFacade, bool setUpSchema = true)
        {
            if (databaseFacade.IsSqlServer())
                //SQL Server
                databaseFacade.CreateExecutionStrategy()
                    .Execute(databaseFacade, database => new SqlServerDatabaseCleaner(databaseFacade).Clean(database, setUpSchema));
            else if (databaseFacade.IsNpgsql())
                //PostgreSQL
                databaseFacade.FasterPostgreSqlEnsureClean(setUpSchema);
            else
                throw new InvalidOperationException("The EnsureClean method only works with SQL Server or PostgreSQL databases.");
        }

        private static void FasterPostgreSqlEnsureClean(this DatabaseFacade databaseFacade, bool setUpSchema = true)
        {
            var connectionString = databaseFacade.GetDbConnection().ConnectionString;
            if (connectionString.DatabaseExists())
            {
                var conn = new NpgsqlConnection(connectionString);
                conn.Open();

                var dropPublicSchemaCommand = new NpgsqlCommand
                {
                    Connection = conn,
                    CommandText = @"
DO $$
DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT nspname FROM pg_namespace WHERE nspname NOT IN ('pg_toast', 'pg_catalog', 'information_schema'))
        LOOP
            EXECUTE 'DROP SCHEMA ' || quote_ident(r.nspname) || ' CASCADE';
        END LOOP;
    EXECUTE 'CREATE SCHEMA public; GRANT ALL ON SCHEMA public TO postgres; GRANT ALL ON SCHEMA public TO public';
END $$"
                };
                dropPublicSchemaCommand.ExecuteNonQuery();
            }

            if (setUpSchema)
                databaseFacade.EnsureCreated();
        }

        private static bool DatabaseExists(this string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var orgDbStartsWith = builder.Database;
            builder.Database = "postgres";
            var newConnectionString = builder.ToString();
            using var conn = new NpgsqlConnection(newConnectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand($"SELECT COUNT(*) FROM pg_catalog.pg_database WHERE datname='{orgDbStartsWith}'", conn);
            return (long)cmd.ExecuteScalar() == 1;
        }
    }
}