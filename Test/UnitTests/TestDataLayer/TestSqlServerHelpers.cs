// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Collections.Generic;
#if NETCOREAPP2_1
using System.Data.SqlClient;
#elif NETCOREAPP3_0
using Microsoft.Data.SqlClient;
#endif
using System.Linq;
using DataLayer.EfCode.BookApp;
using Microsoft.EntityFrameworkCore;
using Test.Helpers;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestSqlServerHelpers 
    {
        private readonly ITestOutputHelper _output;

        public TestSqlServerHelpers(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestExampleSqlDatabaseOk()
        {
            //SETUP
            var options = this
                .CreateUniqueClassOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                context.CreateEmptyViaWipe();

                //ATTEMPT
                context.SeedDatabaseFourBooks();

                //VERIFY
                context.Books.Count().ShouldEqual(4);
            }
        }

        [Fact]
        public void TestSqlServerUniqueClassOk()
        {
            //SETUP
            //ATTEMPT
            var options = this.CreateUniqueClassOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                //VERIFY
                var builder = new SqlConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString);
                builder.InitialCatalog.ShouldEndWith(GetType().Name);
            }
        }

        [Fact]
        public void TestSqlServerUniqueMethodOk()
        {
            //SETUP
            //ATTEMPT
            var options = this.CreateUniqueMethodOptions<BookContext>();
            using (var context = new BookContext(options))
            {

                //VERIFY
                var builder = new SqlConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString);
                builder.InitialCatalog
                    .ShouldEndWith($"{GetType().Name}_{nameof(TestSqlServerUniqueMethodOk)}" );
            }
        }

        [Fact]
        public void TestCreateEmptyViaDeleteOk()
        {
            //SETUP
            var options = this.CreateUniqueMethodOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();
            }
            using (var context = new BookContext(options))
            {
                //ATTEMPT
                using (new TimeThings(_output, "Time to delete and create the database"))
                {
                    context.CreateEmptyViaDelete();
                }

                //VERIFY
                context.Books.Count().ShouldEqual(0);
            }
        }

        [RunnableInDebugOnly]
        public void TestCreateDbToGetLogsOk()
        {
            //SETUP
            var logs = new List<LogOutput>();
            var options = this.CreateUniqueClassOptionsWithLogging<BookContext>(log => logs.Add(log));
            using (var context = new BookContext(options))
            {
                //ATTEMPT
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                //VERIFY
                foreach (var log in logs)
                {
                    _output.WriteLine(log.ToString());
                }
            }
        }

        [Fact]
        public void TestCreateEmptyViaWipe()
        {
            //SETUP
            var options = this.CreateUniqueMethodOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseDummyBooks(100);
            }
            using (var context = new BookContext(options))
            {
                //ATTEMPT
                using(new TimeThings(_output, "Time to wipe the database"))
                {
                    context.CreateEmptyViaWipe();
                }

                //VERIFY
                context.Books.Count().ShouldEqual(0);
            }
        }

#if NETCOREAPP3_0
        [Fact]
        public void TestAddExtraBuilderOptions()
        {
            //SETUP
            var options1 = this.CreateUniqueMethodOptions<BookContext>();
            using (var context = new BookContext(options1))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseDummyBooks(100);

                var book = context.Books.First();
                context.Entry(book).State.ShouldEqual(EntityState.Unchanged);
            }
            //ATTEMPT
            var options2 = this.CreateUniqueMethodOptions<BookContext>(
                builder => builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
            using (var context = new BookContext(options2))
            {
                //VERIFY
                var book = context.Books.First();
                context.Entry(book).State.ShouldEqual(EntityState.Detached);

            }
        }
#endif


    }
}