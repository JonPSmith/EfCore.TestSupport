// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using DataLayer.EfClasses;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.EfCode
{
    public class DbContextOnConfiguring : DbContext
    {
        private readonly string connectionString //#A
            = "Server=(localdb)\\mssqllocaldb;Database=EfCore.TestSupport-Test-OnConfiguring;Trusted_Connection=True";

        public DbContextOnConfiguring(string connectionString) //#B
        {
            this.connectionString = connectionString;
        }

        public DbContextOnConfiguring() //#C
        {
        }

        protected override void OnConfiguring(             //#D
            DbContextOptionsBuilder optionsBuilder)        //#D
        {                                                  //#D
            optionsBuilder.UseSqlServer(connectionString); //#D
            base.OnConfiguring(optionsBuilder);            //#D
        }                                                  //#D

        public DbSet<MyEntity> MyEntities { get; set; }
    }
    /***********************************************************************
    #A I change the connectionString from a constant to a variable, so that the unit test can alter it
    #B I add a constructor that will set a new connection string. This is what my unit test will use
    #C I have to add a parameterless constructor so that the application can create an instance
    #D I don't have to change the OnConfiguring method at all. It now uses the variable connectionString instead of the constant connectionString.
     * ********************************************************************/
}