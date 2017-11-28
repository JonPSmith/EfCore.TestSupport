// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.Issue3
{
    public class Issue3DbContext : DbContext
    {
        public DbSet<Parameter> Parameters { get; set; }

        public Issue3DbContext(
            DbContextOptions<Issue3DbContext> options)      
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parameter>()
                .Property(p => p.ValueAggregationTypeId).HasDefaultValue(ValueAggregationTypeEnum.Invariable);
        }
    }
}