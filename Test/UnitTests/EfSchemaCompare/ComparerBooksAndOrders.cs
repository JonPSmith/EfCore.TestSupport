using System;
using System.Linq;
using DataLayer.EfCode.BookApp;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;
using TestSupport.EfSchemeCompare;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.EfSchemaCompare
{
    public class ComparerBooksAndOrders
    {
        private readonly ITestOutputHelper _output;
        private readonly DbContextOptions<BookOrderContext> _options;
        private readonly string _connectionString;
        public ComparerBooksAndOrders(ITestOutputHelper output)
        {
            _output = output;
            _options = this
                .CreateUniqueClassOptions<BookOrderContext>();

            using (var context = new BookOrderContext(_options))
            {
                _connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
            }
        }

        [Fact]
        public void CompareViaContext()
        {
            //SETUP
            using (var context = new BookOrderContext(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }

        [Fact]
        public void CompareViaConnection()
        {
            //SETUP
            using (var context = new BookOrderContext(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(_connectionString, context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }

        private DbContextOptions<BookContext> GetBookContextOptions()
        {
            var options = this.CreateUniqueMethodOptions<BookContext>();
            return options;
        }

        [Fact]
        public void CompareWithBookDatabase()
        {
            //SETUP
            string connectionString;
            using (var context = new BookContext(GetBookContextOptions()))
            {
                connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
            }

            using (var context = new BookOrderContext(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(connectionString, context);

                //VERIFY
                hasErrors.ShouldBeTrue();
                var errors = CompareLog.ListAllErrors(comparer.Logs).ToList();
                errors.Count.ShouldEqual(2);
                errors[0].ShouldEqual(
                    "NOT IN DATABASE: Entity 'LineItem', table name. Expected = LineItem");
                errors[1].ShouldEqual(
                    "NOT IN DATABASE: Entity 'Order', table name. Expected = Orders");
            }
        }

        [Fact]
        public void CompareBookAgainstBookOrderDatabaseHasErrors()
        {
            //SETUP
            using (var context = new BookContext(GetBookContextOptions()))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(_connectionString, context);

                //VERIFY
                hasErrors.ShouldBeTrue();
                var errors = CompareLog.ListAllErrors(comparer.Logs).ToList();
                errors.Count.ShouldEqual(2);
                errors[0].ShouldEqual(
                    "EXTRA IN DATABASE: Table 'Orders', table name");
                errors[1].ShouldEqual(
                    "EXTRA IN DATABASE: Table 'LineItem', table name");
            }
        }

        [Fact]
        public void CompareBookAgainstBookOrderDatabaseIgnoreTables()
        {
            //SETUP
            using (var context = new BookContext(GetBookContextOptions()))
            {
                var config = new CompareEfSqlConfig
                {
                    TablesToIgnoreCommaDelimited = "Orders,LineItem"
                };
                var comparer = new CompareEfSql(config);

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(_connectionString, context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }

        [Fact]
        public void CompareEfSqlConfigBadTableToIgnore()
        {
            //SETUP
            using (var context = new BookContext(GetBookContextOptions()))
            {
                var config = new CompareEfSqlConfig
                {
                    TablesToIgnoreCommaDelimited = "BadTableName"
                };
                var comparer = new CompareEfSql(config);

                //ATTEMPT
                var ex = Assert.Throws<InvalidOperationException>( () => comparer.CompareEfWithDb(_connectionString, context));

                //VERIFY
                ex.Message.ShouldEqual("The TablesToIgnoreCommaDelimited config property contains a table name of 'BadTableName', which was not found in the database");
            }
        }

        [Fact]
        public void CompareBookThenOrderAgainstBookOrderDatabase()
        {
            //SETUP
            var options = this.CreateUniqueMethodOptions<OrderContext>();
            using (var context1 = new BookContext(GetBookContextOptions()))
            using (var context2 = new OrderContext(options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(_connectionString, context1, context2);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }
    }
}
