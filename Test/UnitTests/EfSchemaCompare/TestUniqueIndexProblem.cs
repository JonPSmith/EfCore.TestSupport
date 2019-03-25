// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using DataLayer.Issue2;
using DataLayer.MyEntityDb.EfCompareDbs;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;
using TestSupport.EfSchemeCompare;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.EfSchemaCompare
{
    public class TestUniqueIndexProblem
    {
        private readonly ITestOutputHelper _output;

        public TestUniqueIndexProblem(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestFindsNormalIndexes()
        {
            //SETUP
            var options = this.CreateUniqueMethodOptionsWithLogging<MyEntityIndexesDbContext>(log => _output.WriteLine(log.ToString()));
            using (var context = new MyEntityIndexesDbContext(options))
            {
                //context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }

        [Fact]
        public void TestFindsConstraintIndexes()
        {
            //SETUP
            var sqlScriptPath = TestData.GetFilePath("Index01*.sql");
            var connectionString = this.GetUniqueDatabaseConnectionString();
            connectionString.WipeCreateDatabase();
            var options = new DbContextOptionsBuilder<MyEntityIndexesDbContext>()
                .UseSqlServer(connectionString)
                .Options; 
            using (var context = new MyEntityIndexesDbContext(options))
            {
                context.ExecuteScriptFileInTransaction(sqlScriptPath);

                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }



    }
}