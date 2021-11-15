using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DataLayer.BookApp.EfCode;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Test.Helpers;
using TestSupport.EfHelpers;

public class Program
{
        private NpgsqlConnection _conn;
    private BookContext _context;

    [GlobalSetup]
    public void Setup()
    {
        NpgsqlConnection.ClearAllPools();

        var options = this.CreatePostgreSqlUniqueDatabaseOptions<BookContext>();
        _context = new BookContext(options);
        _context.Database.EnsureCreated();

        _conn = new NpgsqlConnection(_context.Database.GetDbConnection().ConnectionString);
        _conn.Open();
    }

    //[IterationSetup]
    //public void CreateTables()
    //{
    //    _context.Database.EnsureCreated();
    //}

    [Benchmark]
    public void DropPublicSchemaWithEnsureCreated()
    {
        var dropPublicSchemaBatch = new NpgsqlBatch(_conn)
        {
            BatchCommands =
            {
                new ("DROP SCHEMA public CASCADE"),
                new ("CREATE SCHEMA public"),
                new ("GRANT ALL ON SCHEMA public TO postgres"),
                new ("GRANT ALL ON SCHEMA public TO public")
            }
        };
        dropPublicSchemaBatch.ExecuteNonQuery();
        _context.Database.EnsureCreated();
    }

    [Benchmark]
    public void DropAllSchemasWithEnsureCreated()
    {
        var dropPublicSchemaCommand = new NpgsqlCommand
        {
            Connection = _conn,
            CommandText = @"
DO $$
DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT nspname FROM pg_namespace WHERE nspname NOT IN ('pg_toast', 'pg_catalog', 'information_schema'))
    LOOP
        EXECUTE 'DROP SCHEMA ' || quote_ident(r.nspname) || ' CASCADE';
    END LOOP;
    EXECUTE 'CREATE SCHEMA public';
END $$"
        };
        dropPublicSchemaCommand.ExecuteNonQuery();
        _context.Database.EnsureCreated();
    }

    [Benchmark]
    public void EnsureClean()
    {
        _context.Database.EnsureClean();
    }

    [Benchmark]
    public async Task EnsureCreatedAndWipedByRespawn()
    {
        await _context.EnsureCreatedAndEmptyPostgreSqlAsync();
    }

    static void Main(string[] args) => BenchmarkRunner.Run<Program>();
}
