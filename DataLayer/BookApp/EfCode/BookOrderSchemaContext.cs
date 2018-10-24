// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using DataLayer.BookApp.EfCode.Configurations;
using DataLayer.EfCode.BookApp;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.BookApp.EfCode
{
    public class BookOrderSchemaContext : DbContext
    {
        public DbSet<Book> Books { get; set; }             
        public DbSet<Author> Authors { get; set; }         
        public DbSet<PriceOffer> PriceOffers { get; set; }
        public DbSet<Order> Orders { get; set; }

        public BookOrderSchemaContext(                             
            DbContextOptions<BookOrderSchemaContext> options)      
            : base(options) {}

        protected override void
            OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookConfig());       
            modelBuilder.ApplyConfiguration(new BookAuthorConfig()); 
            modelBuilder.ApplyConfiguration(new PriceOfferConfig());
            modelBuilder.ApplyConfiguration(new LineItemConfig());

            modelBuilder.Entity<Book>().ToTable("DupTable", "BookSchema");
            modelBuilder.Entity<Author>().ToTable("DupTable", "OrderSchema");
        }
    }
}

