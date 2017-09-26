// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

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
            var options = SqliteInMemory         //#A
                .CreateOptions<EfCoreContext>(); //#A
            using (var context = new EfCoreContext(options)) //#B
            {
                context.Database.EnsureCreated(); //#C

                //ATTEMPT
                context.SeedDatabaseFourBooks(); //#D

                //VERIFY
                context.Books.Count().ShouldEqual(4); //#E
            }
        }
        /*************************************************************
        #A Here I call my SqliteInMemory.CreateOptions to provide me with an in-memory database
        #B Now I use that option to create my application's DbContext
        #C I must call the context.Database.EnsureCreated(), which is a special method that creates a database using your application's DbContext and entity classes
        #D Here I run a test method I have written that adds four test books to the database
        #E Here I check that my SeedDatabaseFourBooks worked, and added four books to the database
         * *********************************************************/

        [Fact]
        public void TestSqliteTwoInstancesOk()
        {
            //SETUP
            var options = SqliteInMemory         //#A
                .CreateOptions<EfCoreContext>(); //#A
            using (var context = new EfCoreContext(options))//#B
            {
                context.Database.EnsureCreated();//#C
                context.SeedDatabaseFourBooks(); //#C
            }
            using (var context = new EfCoreContext(options))//#D
            {   
                const string newTitle = "New title";

                //ATTEMPT
                context.Books.First().Title = newTitle; //#E
                context.SaveChanges();                  //#E

                //VERIFY
                context.Books.Select(x => x.Title)  //#F
                    .First().ShouldEqual(newTitle); //#F
            }
        }
        /*************************************************************
        #A I create the in-memory sqlite options in the same was as the last example
        #B I create the first instance of the application's DbContext
        #C I create the database schema and then use my test method to write four books to the database
        #D I close that last instance and open a new instance of the application's DbContext. This means that the new instance does not have any tracked entities which could alter how the test runs
        #E I load the first book, change its title and call SaveChanges to update the database
        #F I select the Title of the First book, and check that it matches my change
         * ***********************************************************/

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