// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using DataLayer.EfCode.BookApp;
using Test.Helpers;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestOptionsWithLogTo
    {
        private readonly ITestOutputHelper _output; 

        public TestOptionsWithLogTo(ITestOutputHelper output) 
        {
            _output = output;
        }

        [Fact]
        public void TestEfCoreLoggingExampleOfOutputToConsole()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptionsWithLogTo<BookContext>(_output.WriteLine);
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();
                
                //ATTEMPT
                var books = context.Books.ToList(); 

                //VERIFY                                    
            }
        }

        [Fact]
        public void TestEfCoreLoggingCheckSqlOutput()
        {
            //SETUP
            var logs = new List<string>();
            var options = SqliteInMemory.CreateOptionsWithLogTo<BookContext>(log => logs.Add(log));
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                //ATTEMPT 
                var book = context.Books.Single(x => x.Reviews.Count() > 1);

                //VERIFY
                logs.Last().ShouldEqual("Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']\r\n" +
                                          "SELECT \"b\".\"BookId\", \"b\".\"Description\", \"b\".\"ImageUrl\", \"b\".\"Price\"," +
                                          " \"b\".\"PublishedOn\", \"b\".\"Publisher\", \"b\".\"SoftDeleted\", \"b\".\"Title\"\r\n" +
                                          "FROM \"Books\" AS \"b\"\r\nWHERE NOT (\"b\".\"SoftDeleted\") AND ((\r\n" +
                                          "    SELECT COUNT(*)\r\n    FROM \"Review\" AS \"r\"\r\n    WHERE \"b\".\"BookId\" = \"r\".\"BookId\") > 1)\r\nLIMIT 2");
            }
        }


    }
}