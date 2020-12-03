// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.SpecialisedEntities;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.MyEntityDb.EfCompareDbs
{
    public class DbQueryDbContext : DbContext
    {
        public DbSet<MyEntity> MyEntities { get; set; }
        public DbSet<MyEntityReadOnly> MyReadOnlyEntities { get; set; }
        public DbQueryDbContext(
            DbContextOptions<DbQueryDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyEntityReadOnly>().HasNoKey();
        }
    }
}