// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.SpecialisedEntities;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.MyEntityDb.EfCompareDbs
{
    public class DbQueryDbContext : DbContext
    {
        public DbSet<MyEntity> MyEntities { get; set; }
#if NETSTANDARD2_0
        public DbQuery<MyEntityReadOnly> MyReadOnlyEntities { get; set; }
#elif NETSTANDARD2_1
        public DbSet<MyEntityReadOnly> MyReadOnlyEntities { get; set; }
#endif
        public DbQueryDbContext(
            DbContextOptions<DbQueryDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
#if NETSTANDARD2_0
            modelBuilder.Entity<MyEntity>().ToTable(nameof(MyReadOnlyEntities));
#elif NETSTANDARD2_1
            modelBuilder.Entity<MyEntityReadOnly>().HasNoKey();
#endif
        }
    }
}