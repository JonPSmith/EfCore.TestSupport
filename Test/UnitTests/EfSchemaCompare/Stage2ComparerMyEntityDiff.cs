using System.Collections.Generic;
using System.Linq;
using DataLayer.MyEntityDb.EfCompareDbs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
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
        private readonly DatabaseModel _databaseModel;
        public Stage2ComparerMyEntityDiff(ITestOutputHelper output)
        {
            _output = output;
            var options = this
                .CreateUniqueClassOptions<MyEntityDbContext>();
            var serviceProvider = new SqlServerDesignTimeServices().GetDesignTimeProvider();
            var factory = serviceProvider.GetService<IDatabaseModelFactory>();

            using (var context = new MyEntityDbContext(options))
            {
                var connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
#if NETCOREAPP2_1
                _databaseModel = factory.Create(connectionString, new string[] { }, new string[] { });
#elif NETCOREAPP3_0
                _databaseModel = factory.Create(connectionString,
                    new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));
#endif
            }
        }

        [Fact]
        public void ExtrasNoErrors()
        {
            //SETUP
            var firstStageLogs =
                JsonConvert.DeserializeObject<List<CompareLog>>(TestData.GetFileContent("DbContextCompareLog01*.json"));
            var handler = new Stage2Comparer(_databaseModel);

            //ATTEMPT
            var hasErrors = handler.CompareLogsToDatabase(firstStageLogs);

            //VERIFY
            hasErrors.ShouldBeFalse();
        }

        [Fact]
        public void ExtrasTable()
        {
            //SETUP
            var jArray = JArray.Parse(TestData.GetFileContent("DbContextCompareLog01*.json"));
            jArray[0]["SubLogs"][0]["Expected"] = "DiffTableName";
            var firstStageLogs = JsonConvert.DeserializeObject<List<CompareLog>>(jArray.ToString());

            var handler = new Stage2Comparer(_databaseModel);

            //ATTEMPT
            var hasErrors = handler.CompareLogsToDatabase(firstStageLogs);

            //VERIFY
            hasErrors.ShouldBeTrue();
            CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                "EXTRA IN DATABASE: Table 'MyEntites'");
        }

        [Fact]
        public void ExtrasProperty()
        {
            //SETUP
            var jArray = JArray.Parse(TestData.GetFileContent("DbContextCompareLog01*.json"));
            jArray[0]["SubLogs"][0]["SubLogs"][0]["Expected"] = "DiffPropName";
            var firstStageLogs = JsonConvert.DeserializeObject<List<CompareLog>>(jArray.ToString());

            var handler = new Stage2Comparer(_databaseModel);

            //ATTEMPT
            var hasErrors = handler.CompareLogsToDatabase(firstStageLogs);

            //VERIFY
            hasErrors.ShouldBeTrue();
            CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                "EXTRA IN DATABASE: Column 'MyEntites', column name. Found = MyEntityId");
        }

        [Fact]
        public void ExtraIndexConstaint()
        {
            //SETUP
            var firstStageLogs = JsonConvert.DeserializeObject<List<CompareLog>>(
                TestData.GetFileContent("DbContextCompareLog01*.json"));

            var options = this.CreateUniqueMethodOptions<MyEntityIndexAddedDbContext>();
            using (var context = new MyEntityIndexAddedDbContext(options))
            {
                var dtService = context.GetDesignTimeService();
                var serviceProvider = dtService.GetDesignTimeProvider();
                var factory = serviceProvider.GetService<IDatabaseModelFactory>();
                var connectionString = context.Database.GetDbConnection().ConnectionString;

                context.Database.EnsureCreated();
#if NETCOREAPP2_1
                var databaseModel = factory.Create(connectionString, new string[] { }, new string[] { });
#elif NETCOREAPP3_0
                var databaseModel = factory.Create(connectionString,
                    new DatabaseModelFactoryOptions(new string[] { }, new string[] { }));
#endif
                var handler = new Stage2Comparer(databaseModel);

                //ATTEMPT
                var hasErrors = handler.CompareLogsToDatabase(firstStageLogs);

                //VERIFY
                hasErrors.ShouldBeTrue();
                CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                    "EXTRA IN DATABASE: Index 'MyEntites', index constraint name. Found = IX_MyEntites_MyInt");
            }
        }
    }
}
