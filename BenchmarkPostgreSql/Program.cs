using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DataLayer.BookApp.EfCode;
using Npgsql;
using Test.Helpers;
using TestSupport.EfHelpers;

public class Program
{
    
    private BookContext _context;

    [GlobalSetup]
    public void Setup()
    {
        NpgsqlConnection.ClearAllPools();

        var options = this.CreatePostgreSqlUniqueClassOptions<BookContext>();
        _context = new BookContext(options);
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

    //[Benchmark]
    //public async Task WipedByRespawnWithCheckForDbExists()
    //{
    //    await _context.EnsureCreatedAndEmptyPostgreSqlAsync();
    //}

    [Benchmark]
    public void EnsureDeletedEnsureCreated()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    static void Main(string[] args) => BenchmarkRunner.Run<Program>();
}
