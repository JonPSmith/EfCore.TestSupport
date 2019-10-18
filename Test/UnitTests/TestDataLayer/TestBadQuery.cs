// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.BookApp;
using DataLayer.EfCode.BookApp;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestBadQuery
    {
        [Fact]
        public void TestSqliteBadQueryOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();  
                context.SeedDatabaseFourBooks();

                //ATTEMPT
                var ex = Assert.Throws<InvalidOperationException>(() => context.Books.Last());

                //VERIFY
#if NETCOREAPP2_1
                ex.Message.ShouldStartWith("Error generated for warning 'Microsoft.EntityFrameworkCore.Query.QueryClientEvaluationWarning:");
#elif NETCOREAPP3_0
                ex.Message.ShouldContain("could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to either");
#endif
            }
        }
    }
}