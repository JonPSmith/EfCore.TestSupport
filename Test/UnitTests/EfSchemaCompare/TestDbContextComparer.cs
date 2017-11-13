using DataLayer.BookApp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.DependencyInjection;
using TestSupport.DesignTimeServices;
using TestSupport.EfHelpers;
using TestSupport.EfSchemeCompare;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.EfSchemaCompare
{
    public class TestDbContextComparer
    {
        private readonly ITestOutputHelper _output;
        private readonly DbContextOptions<EfCoreContext> _options;
        private readonly string _connectionString;
        public TestDbContextComparer(ITestOutputHelper output)
        {
            _output = output;
            _options = this
                .CreateUniqueClassOptions<EfCoreContext>();

            using (var context = new EfCoreContext(_options))
            {
                _connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
            }
        }

        [Fact]
        public void CompareSelfTestEfCoreContext()
        {
            //SETUP
            var serviceProvider = DatabaseProviders.SqlServer.GetDesignTimeProvider();
            var factory = serviceProvider.GetService<IDatabaseModelFactory>();

            using (var context = new EfCoreContext(_options))
            {
                var database = factory.Create(_connectionString, new string[] { }, new string[] { });
                var handler = new DbContextComparer(context);

                //ATTEMPT
                handler.CompareModelToDatabase(database);

                //VERIFY
                handler.Logs.Count.ShouldEqual(1);
                foreach (var log in CompareLog.AllResultsIndented(handler.Logs))
                {
                    _output.WriteLine(log);
                }
            }
        }

    }
}
