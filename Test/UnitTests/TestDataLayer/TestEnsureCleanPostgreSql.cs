// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.BookApp.EfCode;
using DataLayer.Database1;
using DataLayer.Database2;
using DataLayer.MutipleSchema;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Test.Helpers;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestEnsureCleanPostgreSql
    {
        private readonly ITestOutputHelper _output;

        public TestEnsureCleanPostgreSql(ITestOutputHelper output)
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
            var options = this.CreatePostgreSqlUniqueClassOptionsWithLogTo<DbContext1>(log => _output.WriteLine(log), logToOptions);
            using (var context = new DbContext1(options))
            {
                context.Database.EnsureDeleted();

                //ATTEMPT
                logToOptions.ShowLog = true;
                using (new TimeThings(_output, "Time to create a new database"))
                    context.Database.EnsureClean();
                logToOptions.ShowLog = false;

                //VERIFY
                context.Add(new TopClass1());
                context.SaveChanges();
            }
        }

        [Fact]
        public void TestEnsureCreatedThenCreateDatabase1Ok()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueClassOptions<DbContext1>();
            using (var context = new DbContext1(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                using (new TimeThings(_output, "Time to update schema of a database"))
                    context.Database.EnsureClean();

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
            var options = this.CreatePostgreSqlUniqueClassOptionsWithLogTo<DbContext2>(log => _output.WriteLine(log), logToOptions);
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
            var options = this.CreatePostgreSqlUniqueMethodOptions<DbContext1>();
            using (var context = new DbContext1(options))
            {
                context.Database.EnsureCreated();
                context.Add(new TopClass1 { Dependents = new List<Dependent1> { new Dependent1() } });
                context.SaveChanges();
                context.TopClasses.Count().ShouldEqual(1);
                context.Dependents.Count().ShouldEqual(1);

                //ATTEMPT
                using (new TimeThings(_output, "Time to clean a PostgreSQL database"))
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
            var options = this.CreatePostgreSqlUniqueMethodOptions<DbContext2>();
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
            var connectionString = this.GetUniquePostgreSqlConnectionString();
            var builder1 = new DbContextOptionsBuilder<DbContext1>();
            using (var context = new DbContext1(builder1.UseNpgsql(connectionString).Options))
            using (new TimeThings(_output, "EnsureDeleted/EnsureCreated"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
            var builder2 = new DbContextOptionsBuilder<DbContext2>();
            using (var context = new DbContext2(builder2.UseNpgsql(connectionString).Options))
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
        public void TestMutipleSchemaCleared()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueMethodOptions<ManySchemaDbContext>();
            using var context = new ManySchemaDbContext(options);
            context.Database.EnsureCreated();
            context.AddRange(new Class1(), new Class2(), new Class3(), new Class4());
            context.SaveChanges();

            //ATTEMPT
            context.Database.EnsureClean();

            //VERIFY
            context.ChangeTracker.Clear();

            context.Class1s.Any().ShouldBeFalse();
            context.Class2s.Any().ShouldBeFalse();
            context.Class3s.Any().ShouldBeFalse();
            context.Class4s.Any().ShouldBeFalse();

            context.AddRange(new Class1(), new Class2(), new Class3(), new Class4());
            context.SaveChanges();         
        }

        [Fact]
        public void TestEnsureCleanNotSetSchema()
        {
            //SETUP
            var connectionString = this.GetUniquePostgreSqlConnectionString();
            var builder = new DbContextOptionsBuilder<DbContext1>();
            using (var context = new DbContext1(builder.UseNpgsql(connectionString).Options))
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

        //This proves that PostgreSQL EnsureClean does delete the default named migration history table
        [Fact]
        public void TestEnsureCleanApplyMigrationOk()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueClassOptions<BookContext>();
            using var context = new BookContext(options);

            //ATTEMPT      
            context.Database.EnsureClean(false);
            context.Database.Migrate();

            //VERIFY
            context.Books.Count().ShouldEqual(0);
        }

        [Fact]
        public async Task TestRespawnWithCheckDbExists()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueMethodOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureCreated();

            //ATTEMPT
            using (new TimeThings(_output))
                await context.EnsureCreatedAndEmptyPostgreSqlAsync();

            //VERIFY
            context.Books.Count().ShouldEqual(0);
        }

        [Fact]
        public async Task TestRespawnNoCheckDbExists()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueMethodOptions<BookContext>();
            using var context = new BookContext(options);
            context.Database.EnsureCreated();

            //ATTEMPT
            using (new TimeThings(_output))
                await context.EnsureCreatedAndEmptyPostgreSqlAsync(true);

            //VERIFY
            context.Books.Count().ShouldEqual(0);
        }


        private int CountTablesInDatabase(DbContext context)
        {
            return context.Database.ExecuteSqlRaw(
                $"select count(*) from information_schema.tables;");
        }
    }
}