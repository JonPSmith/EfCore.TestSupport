// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using DataLayer.BookApp.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.BookApp
{
    public class OrderContext : DbContext
    {
        public DbSet<Book> Books { get; set; } //#A
        public DbSet<Order> Orders { get; set; }
        //public DbSet<AddressO> Addresses { get; set; }

        public OrderContext(
            DbContextOptions<OrderContext> options)      
            : base(options) { }

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