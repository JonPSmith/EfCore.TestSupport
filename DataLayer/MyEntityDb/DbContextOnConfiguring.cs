// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.MyEntityDb
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
}