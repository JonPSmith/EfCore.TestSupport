// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.EfClasses;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestSqlite
    {

        [Fact]
        public void TestSqliteOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                context.SeedDatabaseFourBooks();

                //VERIFY
                context.Books.Count().ShouldEqual(4);
            }
        }

        [Fact]
        public void TestSqlLiteAcceptsComputedCol()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<DbContextComputedCol>();
            using (var context = new DbContextComputedCol(options))
            {
                //ATTEMPT
                context.Database.EnsureCreated();

                //VERIFY
            }
        }

        [Fact]
        public void TestSqlLiteDoesSupportSchema()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<DbContextWithSchema>();
            using (var context = new DbContextWithSchema(options))
            {
                //ATTEMPT
                context.Database.EnsureCreated();

                //VERIFY
            }
        }

        [Fact]
        public void TestSqlLiteAcceptsComputedColButDoesntWork()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<DbContextComputedCol>();
            using (var context = new DbContextComputedCol(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                context.Add(new MyEntity());
                var ex = Assert.Throws<DbUpdateException>(() => context.SaveChanges());

                //VERIFY
                Assert.StartsWith("SQLite Error 19: 'NOT NULL constraint failed:", ex.InnerException.Message);
            }
        }

    }
}