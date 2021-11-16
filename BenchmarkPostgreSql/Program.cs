using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DataLayer.BookApp.EfCode;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Test.Helpers;
using TestSupport.EfHelpers;

public class Program
{
    private const string _connectionString = "host=localhost;Database=TestSupportBenchmark-TestSupport;Username=postgres;Password=LetMeIn";
    private BookContext _context;

    [GlobalSetup]
    public void Setup()
    {
        NpgsqlConnection.ClearAllPools();

        var builder = new DbContextOptionsBuilder<BookContext>();
        builder.UseNpgsql(_connectionString);
        _context = new BookContext(builder.Options);
        _context.Database.EnsureCreated();
    }

    [IterationSetup]
    public void CreateTables()
    {
        _context.Database.EnsureCreated();
    }

    [Benchmark]
    public void EnsureCleanUsingDropSchema()
    {
        _context.Database.EnsureClean();
    }

    [Benchmark]
    public async Task WipedByRespawnNoCheckDbExists()
    {
        await _context.EnsureCreatedAndEmptyPostgreSqlAsync(true);
    }

    [Benchmark]
    public async Task WipedByRespawnWithCheckForDbExists()
    {
        await _context.EnsureCreatedAndEmptyPostgreSqlAsync();
    }

    [Benchmark]
    public void EnsureDeletedEnsureCreated()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    static void Main(string[] args) => BenchmarkRunner.Run<Program>();
}
