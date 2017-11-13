using System.Linq;
using DataLayer.BookApp.Configurations;
using DataLayer.MyEntityDb.EfCompareDbs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Test.Helpers;
using TestSupport.DesignTimeServices;
using TestSupport.EfHelpers;
using TestSupport.EfSchemeCompare;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.EfSchemaCompare
{
    public class ComparerMyEntityDiff
    {
        private readonly ITestOutputHelper _output;
        private readonly string connectionString;
        private readonly DbContextOptions<MyEntityDbContext> _options;
        private readonly DatabaseModel _databaseModel;
        public ComparerMyEntityDiff(ITestOutputHelper output)
        {
            _output = output;
            _options = this
                .CreateUniqueClassOptions<MyEntityDbContext>();
            var serviceProvider = DatabaseProviders.SqlServer.GetDesignTimeProvider();
            var factory = serviceProvider.GetService<IDatabaseModelFactory>();

            using (var context = new MyEntityDbContext(_options))
            {
                connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
                _databaseModel = factory.Create(connectionString, new string[] { }, new string[] { });
            }
        }

        [Fact]
        public void CompareDefaultConfigNoErrors()
        {
            //SETUP
            using (var context = new MyEntityDbContext(_options))
            {
                var model = context.Model;
                var handler = new DbContextComparer(model, context.GetType().Name);

                //ATTEMPT
                handler.CompareModelToDatabase(_databaseModel);

                //VERIFY
                CompareLog.HadErrors(handler.Logs).ShouldBeFalse();
            }
        }

        [Fact]
        public void CompareSchemaConfig()
        {
            //SETUP
            var optionsBuilder = new DbContextOptionsBuilder<MyEntitySetSchemaDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            using (var context = new MyEntitySetSchemaDbContext(optionsBuilder.Options))
            {
                var handler = new DbContextComparer(context.Model, context.GetType().Name);

                //ATTEMPT
                handler.CompareModelToDatabase(_databaseModel);

                //VERIFY
                CompareLog.HadErrors(handler.Logs).ShouldBeTrue();
                CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                    "NOT IN DATABASE: Entity 'MyEntity', table name. Expected = MySchema.MyEntities");
            }
        }

    }
}
