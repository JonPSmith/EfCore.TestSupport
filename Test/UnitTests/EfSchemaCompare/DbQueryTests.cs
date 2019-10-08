// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.BookApp.EfCode;
using DataLayer.EfCode.BookApp;
using DataLayer.Issue3;
using DataLayer.MyEntityDb.EfCompareDbs;
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
    public class DbQueryTests
    {
        private readonly ITestOutputHelper _output;
        private readonly DbContextOptions<DbQueryDbContext> _options;
        private readonly string _connectionString;
        public DbQueryTests(ITestOutputHelper output)
        {
            _output = output;
            _options = this
                .CreateUniqueClassOptions<DbQueryDbContext>();

            using (var context = new DbQueryDbContext(_options))
            {
                _connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
            }
        }

        [Fact]
        public void TestDbQueryDbContextCompareEfWithDb()
        {
            //SETUP
            using (var context = new DbQueryDbContext(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                hasErrors.ShouldBeTrue();
                comparer.GetAllErrors.ShouldEqual("WARNING: Database 'EfSchemaCompare does not check read-only types'. Expected = <null>, found = MyEntityReadOnly");
            }
        }

    }
}