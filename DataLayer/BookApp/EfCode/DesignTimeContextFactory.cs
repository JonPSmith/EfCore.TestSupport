// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataLayer.BookApp.EfCode
{
    public class DesignTimeContextFactory : IDesignTimeDbContextFactory<BookContext>
    {
        private const string connectionString =
            "Server=(localdb)\\mssqllocaldb;Database=TestSupport;Trusted_Connection=True;MultipleActiveResultSets=true";

        public BookContext CreateDbContext(string[] args)
        {
            var optionsBuilder =
                new DbContextOptionsBuilder<BookContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new BookContext(optionsBuilder.Options);
        }
    }
}
