// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.EfCode.BookApp;
using DataLayer.MyEntityDb;
using DataLayer.MyEntityDb.EfCompareDbs;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestInMemory
    {
        [Fact]
        public void TestInMemoryOk()
        {
            //SETUP
            var options = EfInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                context.SeedDatabaseFourBooks();

                //VERIFY
                context.Books.Count().ShouldEqual(4);
            }
        }

        [Fact]
        public void TestInMemoryAcceptsComputedCol()
        {
            //SETUP
            var options = EfInMemory.CreateOptions<MyEntityComputedColDbContext>();
            using (var context = new MyEntityComputedColDbContext(options))
            {
                //ATTEMPT
                context.Database.EnsureCreated();

                //VERIFY
            }
        }

        [Fact]
        public void TestInMemoryAcceptsComputedColButDoesntWork()
        {
            //SETUP
            var options = EfInMemory.CreateOptions<MyEntityComputedColDbContext>();
            using (var context = new MyEntityComputedColDbContext(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                context.Add(new MyEntity());
                context.SaveChanges();

                //VERIFY
                context.Set<MyEntity>().First().MyDateTime.ShouldEqual(new DateTime());
            }
        }

        [Fact]
        public void TestInMemorySupportsSchema()
        {
            //SETUP
            var options = EfInMemory.CreateOptions<DbContextWithSchema>();
            using (var context = new DbContextWithSchema(options))
            {           
                //ATTEMPT
                context.Database.EnsureCreated();

                //VERIFY
            }
        }
    }
}