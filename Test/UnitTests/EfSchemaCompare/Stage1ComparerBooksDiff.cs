using DataLayer.EfCode.BookApp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.DependencyInjection;
using TestSupport.DesignTimeServices;
using TestSupport.EfHelpers;
using TestSupport.EfSchemeCompare;
using TestSupport.EfSchemeCompare.Internal;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.EfSchemaCompare
{
    public class Stage1ComparerBooksDiff
    {
        private readonly ITestOutputHelper _output;
        private readonly DbContextOptions<BookContext> _options;
        private readonly string _connectionString;
        public Stage1ComparerBooksDiff(ITestOutputHelper output)
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
        public void CompareSelfTestEfCoreContext()
        {
            //SETUP
            using (var context = new BookContext(_options))
            {
                var dtService = context.GetDesignTimeService();
                var serviceProvider = dtService.GetDesignTimeProvider();
                var factory = serviceProvider.GetService<IDatabaseModelFactory>();
#if NETCOREAPP2_1
                var database = factory.Create(_connectionString, new string[] { }, new string[] { });
#elif NETCOREAPP3_0
                var database = factory.Create(_connectionString,
                    new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));
#endif
                var handler = new Stage1Comparer(context.Model, nameof(BookContext));

                //ATTEMPT
                var hasErrors = handler.CompareModelToDatabase(database);

                //VERIFY
                hasErrors.ShouldBeFalse();
                foreach (var log in CompareLog.AllResultsIndented(handler.Logs))
                {
                    _output.WriteLine(log);
                }
            }
        }

    }
}
