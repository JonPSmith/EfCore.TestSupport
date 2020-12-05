// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.MyEntityDb.ModelBuilders;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.MyEntityDb.EfCompareDbs
{
    public class MyEntityIndexesDbContext : DbContext
    {
        public MyEntityIndexesDbContext(DbContextOptions<MyEntityIndexesDbContext> options)                            
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            AddMyEntity.Build(modelBuilder);
            modelBuilder.Entity<MyEntity>().HasIndex(p => p.MyInt).IsUnique().HasDatabaseName("MySpecialName");
            modelBuilder.Entity<MyEntity>().HasIndex(p => p.MyString);
        }
    }
}