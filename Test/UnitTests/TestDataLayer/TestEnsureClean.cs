// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using DataLayer.Database1;
using DataLayer.Database2;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestEnsureClean
    {
        private readonly ITestOutputHelper _output;

        public TestEnsureClean(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestEnsureDeletedThenCreateDatabase1Ok()
        {
            //SETUP
            var showLog = false;
#pragma warning disable 618
            var options = this.CreateUniqueClassOptionsWithLogging<DbContext1>(log =>
#pragma warning restore 618
            {
                if (showLog)
                    _output.WriteLine(log.Message);
            });
            using (var context = new DbContext1(options))
            {
                context.Database.EnsureDeleted();

                //ATTEMPT
                showLog = true;
                context.Database.EnsureClean();
                showLog = false;

                //VERIFY
                context.Add(new TopClass1());
                context.SaveChanges();
            }
        }

        [Fact]
        public void TestEnsureDeletedThenCreateDatabase2Ok()
        {
            //SETUP
            var showLog = false;
#pragma warning disable 618
            var options = this.CreateUniqueClassOptionsWithLogging<DbContext2>(log =>
#pragma warning restore 618
            {
                if (showLog)
                    _output.WriteLine(log.Message);
            });
            using (var context = new DbContext2(options))
            {
                context.Database.EnsureDeleted();

                //ATTEMPT
                showLog = true;
                context.Database.EnsureClean();
                showLog = false;

                //VERIFY
                context.Add(new TopClass2());
                context.SaveChanges();
            }
        }

        [Fact]
        public void TestWipeDataDatabase1Ok()
        {
            //SETUP
            var options = this.CreateUniqueMethodOptions<DbContext1>();
            using (var context = new DbContext1(options))
            {
                context.Database.EnsureCreated();
                context.Add(new TopClass1 { Dependents = new List<Dependent1> { new Dependent1() } });
                context.SaveChanges();
                context.TopClasses.Count().ShouldEqual(1);
                context.Dependents.Count().ShouldEqual(1);

                //ATTEMPT
                context.Database.EnsureClean();

                //VERIFY
                context.TopClasses.Count().ShouldEqual(0);
                context.Dependents.Count().ShouldEqual(0);
            }
        }

        [Fact]
        public void TestWipeDataDatabase2Ok()
        {
            //SETUP
            var options = this.CreateUniqueMethodOptions<DbContext2>();
            using (var context = new DbContext2(options))
            {
                context.Database.EnsureCreated();
                context.Add(new TopClass2 { Dependents = new List<Dependent2> { new Dependent2() } });
                context.SaveChanges();
                context.TopClasses.Count().ShouldEqual(1);
                context.Dependents.Count().ShouldEqual(1);

                //ATTEMPT
                context.Database.EnsureClean();

                //VERIFY
                context.TopClasses.Count().ShouldEqual(0);
                context.Dependents.Count().ShouldEqual(0);
            }
        }

        [Fact]
        public void TestDatabase1SchemaChangeToDatabase2Ok()
        {
            //SETUP
            var connectionString = this.GetUniqueDatabaseConnectionString();
            var builder1 = new DbContextOptionsBuilder<DbContext1>();
            using (var context = new DbContext1(builder1.UseSqlServer(connectionString).Options))
            using (new TimeThings(_output, "EnsureDeleted/EnsureCreated"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
            var builder2 = new DbContextOptionsBuilder<DbContext2>();
            using (var context = new DbContext2(builder2.UseSqlServer(connectionString).Options))
            {
                //ATTEMPT
                using (new TimeThings(_output, "EnsureClean"))
                {
                    context.Database.EnsureClean();
                }

                //VERIFY
                context.Add(new TopClass2());
                context.SaveChanges();
            }
        }

        [Fact]
        public void TestEnsureCleanNotSetSchema()
        {
            //SETUP
            var connectionString = this.GetUniqueDatabaseConnectionString();
            var builder = new DbContextOptionsBuilder<DbContext1>();
            using (var context = new DbContext1(builder.UseSqlServer(connectionString).Options))
            {
                context.Database.EnsureCreated();
                CountTablesInDatabase(context).ShouldNotEqual(0);

                //ATTEMPT-VERIFY1
                context.Database.EnsureClean(false);
                CountTablesInDatabase(context).ShouldEqual(-1);

                //ATTEMPT-VERIFY2
                context.Database.EnsureCreated();
                CountTablesInDatabase(context).ShouldNotEqual(0);
            }
        }

        private int CountTablesInDatabase(DbContext context)
        {
            var databaseName = context.Database.GetDbConnection().Database;
            return context.Database.ExecuteSqlRaw(
                $"USE [{databaseName}] SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'");
        }

    }
}