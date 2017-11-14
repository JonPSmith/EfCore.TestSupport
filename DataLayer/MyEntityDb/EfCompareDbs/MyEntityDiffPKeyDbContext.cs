// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using DataLayer.MyEntityDb.ModelBuilders;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.MyEntityDb.EfCompareDbs
{
    public class MyEntityDiffPKeyDbContext : DbContext
    {
        public MyEntityDiffPKeyDbContext(DbContextOptions<MyEntityDiffPKeyDbContext> options)                            
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            AddMyEntity.Build(modelBuilder);
            modelBuilder.Entity<MyEntity>().HasKey(p => p.MyInt);
        }
    }
}