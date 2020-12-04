// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.BookApp;
using DataLayer.BookApp.EfCode;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestWipeDbViaSql
    {

        [Fact]
        public void TestWipeDbViaSqlOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                //ATTEMPT
#pragma warning disable 618
                context.WipeAllDataFromDatabase();
#pragma warning restore 618

                //VERIFY
                context.Books.Count().ShouldEqual(0);
                context.Authors.Count().ShouldEqual(0);
            }
        }

        [Fact]
        public void TestWipeDbViaSqlNotAuthorsOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                //ATTEMPT
#pragma warning disable 618
                context.WipeAllDataFromDatabase(false, 10, typeof(Author));
#pragma warning restore 618

                //VERIFY
                context.Books.Count().ShouldEqual(0);
                context.Authors.Count().ShouldNotEqual(0);
            }
        }





    }
}