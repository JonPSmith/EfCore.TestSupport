using DataLayer.EfCode.BookApp;
using Microsoft.EntityFrameworkCore;
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
            using (var context = new BookContext(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context, _connectionString);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }

    }
}
