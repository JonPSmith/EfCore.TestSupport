// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.MyEntityDb;
using DataLayer.MyEntityDb.EfCompareDbs;
using Microsoft.Data.Sqlite;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestSqliteLimitations
    {
        [Fact]
        public void TestSqlLiteComputedColDifferent()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<MyEntityComputedColDbContext>();
            using (var context = new MyEntityComputedColDbContext(options))
            {
                //ATTEMPT
                var ex = Assert.Throws<SqliteException>(() => context.Database.EnsureCreated());

                //VERIFY
                ex.Message.ShouldStartWith("SQLite Error 1: 'no such function: getutcdate'.");
            }
        }

        [Fact]
        public void TestSqlLiteDoesAcceptSchema()
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
    }
}