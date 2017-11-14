// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using DataLayer.BookApp.EfCode.Configurations;
using Microsoft.EntityFrameworkCore;
using DataLayer.BookApp;

namespace DataLayer.EfCode.BookApp
{
    public class BookContext : DbContext
    {
        public DbSet<Book> Books { get; set; }             
        public DbSet<Author> Authors { get; set; }         
        public DbSet<PriceOffer> PriceOffers { get; set; } 

        public BookContext(                             
            DbContextOptions<BookContext> options)      
            : base(options) {}

        protected override void
            OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookConfig());       
            modelBuilder.ApplyConfiguration(new BookAuthorConfig()); 
            modelBuilder.ApplyConfiguration(new PriceOfferConfig()); 
        }
    }
}

