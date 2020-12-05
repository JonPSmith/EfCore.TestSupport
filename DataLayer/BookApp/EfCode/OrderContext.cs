// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.BookApp.EfCode.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.BookApp.EfCode
{
    public class OrderContext : DbContext
    {
        public OrderContext(
            DbContextOptions<OrderContext> options)      
            : base(options) { }

        public DbSet<Book> Books { get; set; } //#A
        public DbSet<Order> Orders { get; set; }

        protected override void
            OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookConfig());
            modelBuilder.ApplyConfiguration( new LineItemConfig());

            modelBuilder.Ignore<Review>();    
            modelBuilder.Ignore<PriceOffer>();
            modelBuilder.Ignore<Author>();    
            modelBuilder.Ignore<BookAuthor>();
        }
    }
}