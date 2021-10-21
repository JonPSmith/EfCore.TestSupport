// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using DataLayer.SpecialisedEntities.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.SpecialisedEntities
{
    public class OwnedWithKeyDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public OwnedWithKeyDbContext(DbContextOptions<OwnedWithKeyDbContext> options)      
            : base(options) {}

        protected override void OnModelCreating
            (ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfig());
        }
    }
}

