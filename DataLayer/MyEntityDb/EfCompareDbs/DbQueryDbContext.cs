// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.SpecialisedEntities;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.MyEntityDb.EfCompareDbs
{
    public class DbQueryDbContext : DbContext
    {
        public DbQueryDbContext(
            DbContextOptions<DbQueryDbContext> options)
            : base(options) { }

        public DbSet<MyEntity> MyEntities { get; set; }
        public DbSet<MyEntityReadOnly> MyReadOnlyEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyEntityReadOnly>().HasNoKey();
        }
    }
}