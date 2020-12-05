// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.SpecialisedEntities.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.SpecialisedEntities
{
    public class OwnedWithKeyDbContext : DbContext
    {
        public OwnedWithKeyDbContext(DbContextOptions<OwnedWithKeyDbContext> options)      
            : base(options) {}

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating
            (ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfig());
        }
    }
}

