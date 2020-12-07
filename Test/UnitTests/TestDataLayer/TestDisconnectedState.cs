// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.BookApp;
using DataLayer.BookApp.EfCode;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestDisconnectedState
    {
        [Fact]
        public void TestSqliteSingleInstanceOk()
        {
            //SETUP
            var options = SqliteInMemory
                .CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();  //#A
                context.SeedDatabaseFourBooks();   //#A

                //ATTEMPT
                var book = context.Books.OrderByDescending(x => x.BookId).First();            //#B
                book.Reviews.Add( new Review{NumStars = 5});//#C
                context.SaveChanges();   //#D

                //VERIFY
                context.Books.OrderByDescending(x => x.BookId).First().Reviews
                    .Count.ShouldEqual(3); //#E
            }
        }
        /*********************************************************
        #A I set up the test database with some test data consisting of four books
        #B I read in the last book from my test set, which I know has two reviews
        #C I add another review to the book. THIS SHOULDN'T WORK, but it does because the seed data is still being tracked by the DbContext instance
        #D And save it to the database
        #E I check that I have three reviews, which works, but the unit test should have failed with an exception earlier.
         * *******************************************************/

        [Fact]
        public void TestSqliteTwoInstancesOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            options.StopNextDispose(); 
            using (var context = new BookContext(options))//#B
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks(); //#C
            }
            using (var context = new BookContext(options))//#D
            {
                //ATTEMPT
                var book = context.Books.OrderByDescending(x => x.BookId).First();              //#E
                var ex = Assert.Throws<NullReferenceException>( //#F
                    () => book.Reviews.Add(                     //#F
                        new Review { NumStars = 5 }));          //#F

                //VERIFY
                ex.Message.ShouldStartWith("Object reference not set to an instance of an object.");
            }
        }

        /*************************************************************
        #A I create the in-memory sqlite options in the same way as the last example
        #B I create the first instance of the application's DbContext
        #C I set up the test database with some test data consisting of four books, but this time in a separate DbContext instance
        #D I close that last instance and open a new instance of the application's DbContext. This means that the new instance does not have any tracked entities which could alter how the test runs
        #E I read in the last book from my test set, which I know has two reviews
        #F When I try to add the new Review the EF Core will throw a NullReferenceException
         * ***********************************************************/
    }
}