using System;
using System.Linq;
using DataLayer.BookApp.EfCode;
using DataLayer.EfCode.BookApp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TestSupport.EfHelpers;
using TestSupport.EfSchemeCompare;
using TestSupport.Helpers;
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
        public void CompareBookAgainstBookOrderDatabaseExtraTablesIgnored()
        {
            //SETUP
            using (var context = new BookContext(GetBookContextOptions()))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(_connectionString, context);

                //VERIFY
                hasErrors.ShouldBeFalse();
            }
        }

        [Fact]
        public void CompareBookAgainstBookOrderDatabaseHasErrors()
        {
            //SETUP
            using (var context = new BookContext(GetBookContextOptions()))
            {
                var config = new CompareEfSqlConfig {TablesToIgnoreCommaDelimited = ""};
                var comparer = new CompareEfSql(config);

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(_connectionString, context);

                //VERIFY
                hasErrors.ShouldBeTrue();
                var errors = CompareLog.ListAllErrors(comparer.Logs).ToList();
                errors.Count.ShouldEqual(2);
                errors[0].ShouldEqual(
                    "EXTRA IN DATABASE: EfCore.TestSupport-Test_ComparerBooksAndOrders->Table 'LineItem'");
                errors[1].ShouldEqual(
                    "EXTRA IN DATABASE: EfCore.TestSupport-Test_ComparerBooksAndOrders->Table 'Orders'");
            }
        }

        [Fact]
        public void CompareBookAgainstBookOrderDatabaseSurpressExtraInDatabaseTables()
        {
            //SETUP
            using (var context = new BookContext(GetBookContextOptions()))
            {
                var config = new CompareEfSqlConfig();
                config.AddIgnoreCompareLog(new CompareLog(CompareType.Table, CompareState.ExtraInDatabase, null));
                var comparer = new CompareEfSql(config);

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(_connectionString, context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
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
        public void CompareBookThenOrderAgainstBookOnlyDatabase()
        {
            //SETUP
            var options1 = GetBookContextOptions();
            using (var context = new BookContext(options1))
            {
                context.Database.EnsureCreated();
            }
            var options2 = this.CreateUniqueMethodOptions<OrderContext>();
            using (var context1 = new BookContext(options1))
            using (var context2 = new OrderContext(options2))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb( context1, context2);

                //VERIFY
                hasErrors.ShouldBeTrue(comparer.GetAllErrors);
                comparer.GetAllErrors.ShouldEqual(@"NOT IN DATABASE: Entity 'LineItem', table name. Expected = LineItem
NOT IN DATABASE: Entity 'Order', table name. Expected = Orders");
            }
        }

        [Fact]
        public void CompareBookThenOrderAgainstBookOrderDatabase()
        {
            //SETUP
            var options1 = GetBookContextOptions();
            var options2 = this.CreateUniqueMethodOptions<OrderContext>();
            using (var context1 = new BookContext(options1))
            using (var context2 = new OrderContext(options2))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(_connectionString, context1, context2);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }

        [Fact]
        public void CompareDatabaseViaConnectionName()
        {
            //SETUP
            const string connectionStringName = "BookOrderConnection"; //#A
            //!!!!!!!!!! LEAVE OUT OF BOOK - START
            var connectionString = AppSettings.GetConfiguration().GetConnectionString(connectionStringName);
            var optionsBuilder = new DbContextOptionsBuilder<BookOrderContext>();
            optionsBuilder.UseSqlServer(connectionString);
            var options = optionsBuilder.Options;
            //!!!!!!!!!! LEAVE OUT OF BOOK - END
            //... I left out the option building part to save space
            using (var context = new BookOrderContext(options)) //#B
            {
                var comparer = new CompareEfSql(); //#C

                //ATTEMPT
                bool hasErrors = comparer.CompareEfWithDb //#D
                    (connectionStringName, context);     //#D

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors); //#E
            }
        }
        /************************************************************
        #A I have added connection string called "BookOrderConnection" to my unit tests's appsettings.json file. This points to my development database
        #B I create an instance of my applictaion's DbContext, which will contain the latest entity classes and EF Core configuration
        #C I create the CompareEfSql. It can have various configurations set, but in this case I use the default seetings
        #D I use the version of the CompareEfWithDb method that takes a connection string, or a connection string name. In this case I am providing a connection string name, which it looks up in the appsettings.json file
        #E The hasErrors variable will be true if there were differences. If there are the ShouldBeFalse fluent assert will fail and output the string given in the parameter. The comparer.GetAllErrors property returns a string, with each difference on a separate line
         * **********************************************************/


        [Fact]
        public void CompareBookThenOrderAgainstBookOrderDatabaseViaAppSettings()
        {
            //SETUP
            var options1 = GetBookContextOptions();
            var options2 = this.CreateUniqueMethodOptions<OrderContext>();
            const string connectionStringName = "BookOrderConnection";
            using (var context1 = new BookContext(options1))
            using (var context2 = new OrderContext(options2))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(connectionStringName, context1, context2);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }
    }
}
