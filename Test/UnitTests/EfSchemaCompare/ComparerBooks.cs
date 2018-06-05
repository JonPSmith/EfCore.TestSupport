using DataLayer.EfCode.BookApp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using TestSupport.EfSchemeCompare;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.EfSchemaCompare
{
    public class ComparerBooks
    {
        private readonly ITestOutputHelper _output;
        private readonly DbContextOptions<BookContext> _options;
        private readonly string _connectionString;
        public ComparerBooks(ITestOutputHelper output)
        {
            _output = output;
            _options = this
                .CreateUniqueClassOptions<BookContext>();

            using (var context = new BookContext(_options))
            {
                _connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
            }
        }

        [Fact]
        public void CompareViaContext()
        {
            //SETUP
            using (var context = new BookContext(_options))
            {
                context.Database.EnsureCreated();
                var comparer = new CompareEfSql();

                //ATTEMPT
                //This will compare EF Core model of the database 
                //with the database that the context's connection points to
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                //The CompareEfWithDb method returns true if there were errors. 
                //The comparer.GetAllErrors property returns a string
                //where each error is on a separate line
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }

        [Fact]
        public void CompareViaConnection()
        {
            //SETUP
            using (var context = new BookContext(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                using (new TimeThings(_output, "Time to compare simple database"))
                {
                    var hasErrors = comparer.CompareEfWithDb(_connectionString, context);

                    //VERIFY
                    hasErrors.ShouldBeFalse(comparer.GetAllErrors);
                }
            }
        }

        [Fact]
        public void CompareViaType()
        {
            //SETUP
            using (var context = new BookContext(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb<SqlServerDesignTimeServices>(context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }

        [Fact]
        public void CompareViaTypeWithConnection()
        {
            //SETUP
            using (var context = new BookContext(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb<SqlServerDesignTimeServices>(_connectionString, context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }

        [Fact]
        public void CompareBadConnection()
        {
            //SETUP
            using (var context = new BookContext(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT

                var ex = Assert.Throws<System.ArgumentException>(() =>
                comparer.CompareEfWithDb("bad connection string", context));

                //VERIFY
                ex.Message.ShouldEqual("Format of the initialization string does not conform to specification starting at index 0.");
            }
        }

        [RunnableInDebugOnly]
        public void CompareConnectionBadDatabase()
        {
            //SETUP
            var badDatabaseConnection =
                "Server=(localdb)\\mssqllocaldb;Database=BadDatabaseName;Trusted_Connection=True;MultipleActiveResultSets=true";
            using (var context = new BookContext(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var ex = Assert.Throws<System.Data.SqlClient.SqlException>(() =>
                    comparer.CompareEfWithDb(badDatabaseConnection, context));

                //VERIFY
                ex.Message.ShouldStartWith("Cannot open database \"BadDatabaseName\" requested by the login. The login failed");
            }
        }

    }
}
