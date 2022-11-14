// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using DataLayer.BookApp.EfCode;
using DataLayer.Database1;
using DataLayer.Database2;
using Microsoft.EntityFrameworkCore;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestEnsureCleanSqlServer
    {
        private readonly ITestOutputHelper _output;

        public TestEnsureCleanSqlServer(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestEnsureDeletedThenCreateDatabase1Ok()
        {
            //SETUP
            var logToOptions = new LogToOptions
            {
                ShowLog = false
            };
            var options = this.CreateUniqueClassOptionsWithLogTo<DbContext1>(log => _output.WriteLine(log), logToOptions);
            using (var context = new DbContext1(options))
            {
                context.Database.EnsureDeleted();

                //ATTEMPT
                logToOptions.ShowLog = true;
                using (new TimeThings(_output, "Time to create a database"))
                    context.Database.EnsureClean();
                logToOptions.ShowLog = false;

                //VERIFY
                context.Add(new TopClass1());
                context.SaveChanges();
            }
        }

        [Fact]
        public void TestEnsureDeletedThenCreateDatabase2Ok()
        {
            //SETUP
            var logToOptions = new LogToOptions
            {
                ShowLog = false
            };
            var options = this.CreateUniqueClassOptionsWithLogTo<DbContext2>(log => _output.WriteLine(log), logToOptions);
            using (var context = new DbContext2(options))
            {
                context.Database.EnsureDeleted();

                //ATTEMPT
                logToOptions.ShowLog = true;
                context.Database.EnsureClean();
                logToOptions.ShowLog = false;

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
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                context.Add(new TopClass1 { Dependents = new List<Dependent1> { new Dependent1() } });
                context.SaveChanges();
                context.TopClasses.Count().ShouldEqual(1);
                context.Dependents.Count().ShouldEqual(1);

                //ATTEMPT
                using (new TimeThings(_output, "Time to clean a SQL database"))
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
                context.Database.EnsureDeleted();
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

        //This proves that SQL Server EnsureClean doesn't delete the default named migration history table
        //but it does delete migration history tables 
        //To check that it doesn't delete the default named migration history table remove the
        //MigrationsHistoryTable settings. If you run it twice it will NOT update the database schema because its already applied
        [RunnableInDebugOnly]
        public void TestEnsureCleanApplyMigrationOk()
        {
            //SETUP
            var connectionString = this.GetUniqueDatabaseConnectionString();
            var optionsBuilder = new DbContextOptionsBuilder<BookContext>();
            optionsBuilder.UseNpgsql(connectionString, dbOptions =>
                dbOptions.MigrationsHistoryTable("BookMigrationHistoryName"));
            using var context = new BookContext(optionsBuilder.Options);

            //ATTEMPT      
            context.Database.EnsureClean(false);
            context.Database.Migrate();

            //VERIFY
            context.Books.Count().ShouldEqual(0);
        }

        private int CountTablesInDatabase(DbContext context)
        {
            var databaseName = context.Database.GetDbConnection().Database;
            return context.Database.ExecuteSqlRaw(
                $"USE [{databaseName}] SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'");
        }
    }
}