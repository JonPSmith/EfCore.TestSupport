// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataLayer.BookApp.EfCode
{
    public class PostgreSqlContextFactory : IDesignTimeDbContextFactory<BookContext>
    {
        private const string connectionString =
            "host=127.0.0.1;Database=TestSupport-Migrate;Username=postgres;Password=LetMeIn";

        public BookContext CreateDbContext(string[] args)
        {
            var optionsBuilder =
                new DbContextOptionsBuilder<BookContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new BookContext(optionsBuilder.Options);
        }
    }
}
