
using Microsoft.EntityFrameworkCore;

namespace DataLayer.MutipleSchema
{
    public class ManySchemaDbContext : DbContext
    {
        public ManySchemaDbContext(DbContextOptions<ManySchemaDbContext> options)
            : base(options) { }

        public DbSet<Class1> Class1s { get; set; }
        public DbSet<Class2> Class2s { get; set; }
        public DbSet<Class3> Class3s { get; set; }
        public DbSet<Class4> Class4s { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Class2>().ToTable("Class2s", schema: "Schema2");
            modelBuilder.Entity<Class3>().ToTable("Class3s", schema: "Schema3");
            modelBuilder.Entity<Class4>().ToTable("Class4s", schema: "Schema4");
        }

    }
}
