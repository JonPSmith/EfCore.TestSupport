// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.BookApp.EfCode.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.BookApp.EfCode
{
    public class BookContext : DbContext
    {
        public BookContext(                             
            DbContextOptions<BookContext> options)      
            : base(options) {}

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<PriceOffer> PriceOffers { get; set; }

        protected override void
            OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookConfig());       
            modelBuilder.ApplyConfiguration(new BookAuthorConfig()); 
            modelBuilder.ApplyConfiguration(new PriceOfferConfig()); 
        }
    }

    /******************************************************************************
    * NOTES ON MIGRATION:
    * 
    * see https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/migrations?tabs=visual-studio
    * 
    * The following NuGet libraries must be loaded
    * 1. Add to DataLayer: "Microsoft.EntityFrameworkCore.Tools"
    * 2. Add to DataLayer: "Microsoft.EntityFrameworkCore.SqlServer" (or another database provider)
    * 
    * 2. Using Package Manager Console commands
    * The steps are:
    * a) Make sure the default project is DataLayer
    * b) Use the PMC command
    *    Add-Migration Initial -Context BookContext -OutputDir BookApp
    *    
    * If you want to start afresh then:
    * a) Delete the current database
    * b) Delete all the class in the Migration directory
    * c) follow the steps to add a migration
    ******************************************************************************/
}

