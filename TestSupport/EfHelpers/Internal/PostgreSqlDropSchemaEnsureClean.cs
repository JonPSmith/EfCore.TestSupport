using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using TestSupport.Helpers;

namespace TestSupport.EfHelpers.Internal
{
    internal static class PostgreSqlDropSchemaEnsureClean
    {
        /// <summary>
        /// This uses the "DROP SCHEMA" approach (see https://stackoverflow.com/a/13823560/1434764) 
        /// to remove all the tables, extensions, functions, collations.
        /// The SQL in this method was provided by Shay Rojansky, github @roji
        /// </summary>
        /// <param name="databaseFacade"></param>
        /// <param name="password"></param>
        /// <param name="setUpSchema"></param>
        public static void FasterPostgreSqlEnsureClean(this DatabaseFacade databaseFacade, string password, bool setUpSchema = true)
        {
            var builder = new NpgsqlConnectionStringBuilder(databaseFacade.GetDbConnection().ConnectionString)
            {
                Password = password
            };
            var connectionString = builder.ToString();

            if (connectionString.DatabaseExists())
            {
                using var conn = new NpgsqlConnection(connectionString);
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
    EXECUTE 'CREATE SCHEMA public; GRANT ALL ON SCHEMA public TO current_user; GRANT ALL ON SCHEMA public TO public';
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
