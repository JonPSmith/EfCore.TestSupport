// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfClasses;
using DataLayer.EfCode;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestWipeDbViaSql
    {

        [Fact]
        public void TestWipeDbViaSqlOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                //ATTEMPT
                context.WipeAllDataFromDatabase();

                //VERIFY
                context.Books.Count().ShouldEqual(0);
                context.Authors.Count().ShouldEqual(0);
            }
        }

        [Fact]
        public void TestWipeDbViaSqlNotAuthorsOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                //ATTEMPT
                context.WipeAllDataFromDatabase(false, 10, typeof(Author));

                //VERIFY
                context.Books.Count().ShouldEqual(0);
                context.Authors.Count().ShouldNotEqual(0);
            }
        }





    }
}