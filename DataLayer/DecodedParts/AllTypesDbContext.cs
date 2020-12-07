// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.DecodedParts
{
    public class AllTypesDbContext : DbContext
    {
        public AllTypesDbContext(DbContextOptions<AllTypesDbContext> options)      
            : base(options) {}

        public DbSet<AllTypesEntity> AllTypesEntities { get; set; }

        protected override void OnModelCreating
            (ModelBuilder modelBuilder)
        {
        }
    }
}

