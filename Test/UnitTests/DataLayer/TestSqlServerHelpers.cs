// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Data.SqlClient;
using System.Linq;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestSqlServerHelpers
    {
        private readonly ITestOutputHelper _output;

        public TestSqlServerHelpers(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestSqlServerUniqueClassOk()
        {
            //SETUP
            //ATTEMPT
            var options = this.CreateUniqueClassOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
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
            var options = this.CreateUniqueMethodOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {

                //VERIFY
                var builder = new SqlConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString);
                builder.InitialCatalog
                    .ShouldEndWith($"{GetType().Name}.{nameof(TestSqlServerUniqueMethodOk)}" );
            }
        }

        [Fact]
        public void TestCreateEmptyViaDeleteOk()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();
            }
            using (var context = new EfCoreContext(options))
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

        [Fact]
        public void TestCreateEmptyViaWipe()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseDummyBooks(100);
            }
            using (var context = new EfCoreContext(options))
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

    }
}