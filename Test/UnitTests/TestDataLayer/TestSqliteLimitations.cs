// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.DiffConfig;
using Microsoft.Data.Sqlite;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestSqliteLimitations
    {
        [Fact]
        public void TestSqlLiteDoesAcceptSchema()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<DiffConfigDbContext>();
            using (var context = new DiffConfigDbContext(options, DiffConfigs.AddSchema))
            {
                //ATTEMPT
                context.Database.EnsureCreated();

                //VERIFY
            }
        }

        [Fact]
        public void TestSqlLiteDoesAcceptAddSequence()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<DiffConfigDbContext>();
            using (var context = new DiffConfigDbContext(options, DiffConfigs.AddSequence))
            {
                //ATTEMPT
                context.Database.EnsureCreated();

                //VERIFY
            }
        }


        [Fact]
        public void TestSqlLiteComputedColDifferent()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<DiffConfigDbContext>();
            using (var context = new DiffConfigDbContext(options, DiffConfigs.SetComputedCol))
            {
                //ATTEMPT
                var ex = Assert.Throws<SqliteException>(() => context.Database.EnsureCreated());

                //VERIFY
                ex.Message.ShouldStartWith("SQLite Error 1: 'no such function: getutcdate'.");
            }
        }


    }
}