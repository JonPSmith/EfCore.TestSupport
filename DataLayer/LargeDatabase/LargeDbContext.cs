using Microsoft.EntityFrameworkCore;

namespace DataLayer.LargeDatabase
{
    public class LargeDbContext : DbContext
    {
        public LargeDbContext(DbContextOptions<LargeDbContext> options)
            : base(options) { }

        public DbSet<SharedEntity> Table001 => Set<SharedEntity>("Table1");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            for(int i = 0; i < 100; i++)
                modelBuilder.SharedTypeEntity<SharedEntity>($"Table{i:D3}");
        }

    }
}
