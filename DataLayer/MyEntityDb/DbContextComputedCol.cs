// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.MyEntityDb
{
    public class DbContextComputedCol : DbContext
    {
        public DbSet<MyEntity> MyEntities { get; set; }

        public DbContextComputedCol(
            DbContextOptions<DbContextComputedCol> options)      
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyEntity>()
                .Property(p => p.MyDateTime)
                .HasComputedColumnSql("getutcdate()");
        }
    }
}