// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.BookApp.EfCode;
using DataLayer.EfCode.BookApp;
using DataLayer.Issue3;
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
    public class Issue012Tests
    {
        private readonly ITestOutputHelper _output;
        private readonly DbContextOptions<BookOrderSchemaContext> _options;
        private readonly string _connectionString;
        public Issue012Tests(ITestOutputHelper output)
        {
            _output = output;
            _options = this
                .CreateUniqueClassOptions<BookOrderSchemaContext>();

            using (var context = new BookOrderSchemaContext(_options))
            {
                _connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
            }
        }

        [Fact]
        public void TestTwoTablesWithSameNameButDifferentSchemas()
        {
            //SETUP
            using (var context = new BookOrderSchemaContext(_options))
            {
                var dtService = context.GetDesignTimeService();
                var serviceProvider = dtService.GetDesignTimeProvider();
                var factory = serviceProvider.GetService<IDatabaseModelFactory>();
                var database = factory.Create(_connectionString, new string[] { }, new string[] { });
                var handler = new Stage1Comparer(context.Model, nameof(BookOrderSchemaContext));

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