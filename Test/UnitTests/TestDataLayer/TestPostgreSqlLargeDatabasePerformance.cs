// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.BookApp.EfCode;
using DataLayer.LargeDatabase;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Test.Helpers;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestPostgreSqlLargeDatabasePerformance
    {
        private readonly ITestOutputHelper _output;

        public TestPostgreSqlLargeDatabasePerformance(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestLargeDbEnsureDeletedEnsureCreatedOk()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueDatabaseOptions<LargeDbContext>();
            using var context = new LargeDbContext(options);

            context.Database.EnsureCreated();
  
            //ATTEMPT
            using (new TimeThings(_output, "LargeDatabase to EnsureDeleted and EnsureCreated"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            //VERIFY
        }


        [Fact]
        public void TestLargeDbEnsureCleanExistingDatabaseOk()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueDatabaseOptions<LargeDbContext>();
            using var context = new LargeDbContext(options);

            context.Database.EnsureCreated();

            //ATTEMPT
            using (new TimeThings(_output, "LargeDatabase to EnsureClean"))
            {
                context.Database.EnsureClean();
            }

            //VERIFY
        }

        [Fact]
        public async Task TestLargeDbEnsureCreatedAndEmptyPostgreSqlOk()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueDatabaseOptions<LargeDbContext>();
            using var context = new LargeDbContext(options);

            context.Database.EnsureCreated();

            //ATTEMPT
            using (new TimeThings(_output, "LargeDatabase to empty database"))
            {
                await context.EnsureCreatedAndEmptyPostgreSqlAsync();
            }

            //VERIFY
        }
    }
}