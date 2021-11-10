// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.BookApp.EfCode;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Test.Helpers;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestPostgreSqlHelpers 
    {
        private readonly ITestOutputHelper _output;

        public TestPostgreSqlHelpers(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestPostgreSqlUniqueClassOk()
        {
            //SETUP
            //ATTEMPT
            var options = this.CreatePostgreSqlUniqueDatabaseOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                //VERIFY
                var builder = new NpgsqlConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString);
                builder.Database.ShouldEndWith(GetType().Name);
            }
        }

        [Fact]
        public void TestPostgreSqUniqueMethodOk()
        {
            //SETUP
            //ATTEMPT
            var options = this.CreatePostgreSqlUniqueMethodOptions<BookContext>();
            using (var context = new BookContext(options))
            {

                //VERIFY
                var builder = new NpgsqlConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString);
                builder.Database
                    .ShouldEndWith($"{GetType().Name}_{nameof(TestPostgreSqUniqueMethodOk)}" );
            }
        }

        [Fact]
        public void TestEnsureDeletedEnsureCreatedOk()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueDatabaseOptions<BookContext>();
            using var context = new BookContext(options);

            context.Database.EnsureCreated();
            context.SeedDatabaseFourBooks();

            //ATTEMPT
            using (new TimeThings(_output, "Time to EnsureDeleted and EnsureCreated"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            //VERIFY
            context.Books.Count().ShouldEqual(0);
        }

        [Fact]
        public void TestEnsureCleanExistingDatabaseOk()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueDatabaseOptions<BookContext>();
            using var context = new BookContext(options);

            context.Database.EnsureCreated(); 
            context.SeedDatabaseFourBooks();

            //ATTEMPT
            using (new TimeThings(_output, "Time to EnsureClean"))
            {
                context.Database.EnsureClean();
            }

            //VERIFY
            context.Books.Count().ShouldEqual(0);
        }


        //This proves that PostgreSQL EnsureClean doesn't delete the default named migration history table
        //but it does delete migration history tables 
        //To check that it doesn't delete the default named migration history table remove the
        //MigrationsHistoryTable settings. If you run it twice it will NOT update the database schema because its already applied
        [RunnableInDebugOnly]
        public void TestEnsureCleanApplyMigrationOk()
        {
            //SETUP
            var connectionString = this.GetUniquePostgreSqlConnectionString();
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

        [Fact]
        public void TestEnsureCleanNoExistingDatabaseOk()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueDatabaseOptions<BookContext>();
            using var context = new BookContext(options);

            context.Database.EnsureDeleted(); 

            //ATTEMPT
            using (new TimeThings(_output, "Time to EnsureClean"))
            {
                context.Database.EnsureClean();
            }

            //VERIFY
            context.Books.Count().ShouldEqual(0);
        }

        [Fact]
        public async Task TestEnsureCreatedAndEmptyPostgreSqlOk()
        {
            //SETUP
            var logOptions = new LogToOptions
            {
                ShowLog = false,
                LogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                LoggerOptions = Microsoft.EntityFrameworkCore.Diagnostics.DbContextLoggerOptions.UtcTime
            };
            var options = this.CreatePostgreSqlUniqueClassOptionsWithLogTo<BookContext>(log => _output.WriteLine(log), logOptions);
            using (var context = new BookContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();
            }
            using (var context = new BookContext(options))
            {
                //ATTEMPT
                //logOptions.ShowLog = true;
                using (new TimeThings(_output, "Time to empty database"))
                {
                    await context.EnsureCreatedAndEmptyPostgreSqlAsync();
                }
                logOptions.ShowLog = false;

                //VERIFY
                context.Books.Count().ShouldEqual(0);
            }
        }

        [RunnableInDebugOnly]
        public void TestCreatePostgreSqlUniqueClassOptionsWithLogToOk()
        {
            //SETUP
            var logs = new List<string>();
            var options = this.CreatePostgreSqlUniqueClassOptionsWithLogTo<BookContext>(log => logs.Add(log));
            using (var context = new BookContext(options))
            {
                //ATTEMPT
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                //VERIFY
                foreach (var log in logs)
                {
                    _output.WriteLine(log);
                }
            }
        }

        [Fact]
        public void TestAddExtraBuilderOptions()
        {
            //SETUP
            var options1 = this.CreatePostgreSqlUniqueDatabaseOptions<BookContext>();
            using (var context = new BookContext(options1))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseDummyBooks(100);

                var book = context.Books.First();
                context.Entry(book).State.ShouldEqual(EntityState.Unchanged);
            }
            //ATTEMPT
            var options2 = this.CreatePostgreSqlUniqueDatabaseOptions<BookContext>(
                builder => builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
            using (var context = new BookContext(options2))
            {
                //VERIFY
                var book = context.Books.First();
                context.Entry(book).State.ShouldEqual(EntityState.Detached);
            }
        }
    }
}