// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.SpecialisedEntities.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.SpecialisedEntities
{
    public class SpecializedDbContext : DbContext
    {
        public SpecializedDbContext(DbContextOptions<SpecializedDbContext> options)      
            : base(options) {}

        public DbSet<BookSummary> BookSummaries { get; set; }
        public DbSet<OrderInfo> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<AllTypesEntity> AllTypesEntities { get; set; }

        protected override void OnModelCreating
            (ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookSummaryConfig());
            modelBuilder.ApplyConfiguration(new BookDetailConfig());
            modelBuilder.ApplyConfiguration(new OrderInfoConfig());
            modelBuilder.ApplyConfiguration(new PaymentConfig());
        }
    }
}

