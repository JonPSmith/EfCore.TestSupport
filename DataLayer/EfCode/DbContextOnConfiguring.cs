// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using DataLayer.EfClasses;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.EfCode
{
    public class DbContextOnConfiguring : DbContext
    {
        private const string ConnectionString
            = "Server=(localdb)\\mssqllocaldb;Database=EfCore.TestSupport-Test-OnConfiguring;Trusted_Connection=True";

        protected override void OnConfiguring(             
            DbContextOptionsBuilder optionsBuilder)        
        {      
            if (!optionsBuilder.IsConfigured)    //#A
            {
                optionsBuilder
                    .UseSqlServer(ConnectionString);  
            }          
        }

        public DbContextOnConfiguring(               //#B
            DbContextOptions<DbContextOnConfiguring> //#B
            options)                                 //#B
            : base(options) { }                      //#B

        public DbContextOnConfiguring() { } //#C


        public DbSet<MyEntity> MyEntities { get; set; }
    }
    /***********************************************************************
    #A I change the OnConfigured method to only run its normal setup code if the options aren't already configured
    #B I then add the same constructor-based options settings that the ASP.NET Core version has, which allows me to set any options I want
    #C I need to add a public, parameterless constructor so that this DbContext will work normally with the application
     * ********************************************************************/
}