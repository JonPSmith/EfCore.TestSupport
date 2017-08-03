// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.EfClasses;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using test.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestInMemory
    {
        [Fact]
        public void TestInMemoryAcceptsComputedCol()
        {
            //SETUP
            var options = EfInMemory.CreateNewContextOptions<DbContextComuptedCol>();

            using (var context = new DbContextComuptedCol(options))
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
            var options = EfInMemory.CreateNewContextOptions<DbContextComuptedCol>();

            using (var context = new DbContextComuptedCol(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                context.Add(new MyEntity());
                context.SaveChanges();

                //VERIFY
                context.MyEntities.First().MyDateTime.ShouldEqual(new DateTime());
            }
        }

        [Fact]
        public void TestInMemorySupportsSchema()
        {
            //SETUP
            var options = EfInMemory.CreateNewContextOptions<DbContextWithSchema>();

            using (var context = new DbContextWithSchema(options))
            {           
                //ATTEMPT
                context.Database.EnsureCreated();

                //VERIFY
            }
        }
    }
}