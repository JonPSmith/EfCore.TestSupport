using System.Linq;
using DataLayer.SpecialisedEntities;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;
using TestSupport.EfSchemeCompare;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.EfSchemaCompare
{
    public class ComparerSpecialized
    {
        private readonly ITestOutputHelper _output;
        private readonly DbContextOptions<SpecializedDbContext> _options;
        private readonly string _connectionString;
        public ComparerSpecialized(ITestOutputHelper output)
        {
            _output = output;
            _options = this
                .CreateUniqueClassOptions<SpecializedDbContext>();

            using (var context = new SpecializedDbContext(_options))
            {
                _connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
            }
        }

        [Fact]
        public void CompareViaContext()
        {
            //SETUP
            using (var context = new SpecializedDbContext(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }
    }
}
