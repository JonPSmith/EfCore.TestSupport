using System.Collections.Generic;
using System.Linq;
using DataLayer.MyEntityDb.EfCompareDbs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestSupport.DesignTimeServices;
using TestSupport.EfHelpers;
using TestSupport.EfSchemeCompare;
using TestSupport.EfSchemeCompare.Internal;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.EfSchemaCompare
{
    public class Stage2ComparerMyEntityDiff
    {
        private readonly ITestOutputHelper _output;
        private readonly string _connectionString;
        private readonly DbContextOptions<MyEntityDbContext> _options;
        private readonly DatabaseModel _databaseModel;
        public Stage2ComparerMyEntityDiff(ITestOutputHelper output)
        {
            _output = output;
            _options = this
                .CreateUniqueClassOptions<MyEntityDbContext>();
            var serviceProvider = DatabaseProviders.SqlServer.GetDesignTimeProvider();
            var factory = serviceProvider.GetService<IDatabaseModelFactory>();

            using (var context = new MyEntityDbContext(_options))
            {
                _connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
                _databaseModel = factory.Create(_connectionString, new string[] { }, new string[] { });
            }
        }

        [Fact]
        public void ExtrasNoErrors()
        {
            //SETUP
            var firstStageLogs =
                JsonConvert.DeserializeObject<List<CompareLog>>(TestData.GetFileContent("DbContextCompareLog01*.json"));
            var handler = new ExtraInDatabaseComparer(_databaseModel);

            //ATTEMPT
            var hasErrors = handler.CompareLogsToDatabase(firstStageLogs);

            //VERIFY
            hasErrors.ShouldBeFalse();
        }

        [Fact]
        public void ExtrasMissingTableNoErrors()
        {
            //SETUP
            var jArray = JArray.Parse(TestData.GetFileContent("DbContextCompareLog01*.json"));
            jArray[0]["SubLogs"][0]["Expected"] = "DiffTableName";
            var firstStageLogs = JsonConvert.DeserializeObject<List<CompareLog>>(jArray.ToString());

            var handler = new ExtraInDatabaseComparer(_databaseModel);

            //ATTEMPT
            var hasErrors = handler.CompareLogsToDatabase(firstStageLogs);

            //VERIFY
            hasErrors.ShouldBeTrue();
            CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                "EXTRA IN DATABASE: Table 'DiffTableName', table name");
        }



    }
}
