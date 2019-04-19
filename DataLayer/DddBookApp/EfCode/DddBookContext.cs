// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using DataLayer.DddBookApp.EfCode.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.DddBookApp.EfCode
{
    public class DddBookContext : DbContext
    {
        public DddBookContext(                             
            DbContextOptions<DddBookContext> options)      
            : base(options) {}

        public DbSet<DddBook> DddBooks { get; set; }
        public DbSet<DddAuthor> DddAuthors { get; set; }

        protected override void
            OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookConfig());       
            modelBuilder.ApplyConfiguration(new BookAuthorConfig());  
        }
    }
}

