// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using DataLayer.BookApp.EfCode;
using DataLayer.Issue15;
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
    public class Issue015Tests
    {
        private readonly ITestOutputHelper _output;
        private readonly DbContextOptions<Issue15DbContext> _options;
        private readonly string _connectionString;
        public Issue015Tests(ITestOutputHelper output)
        {
            _output = output;
            _options = this
                .CreateUniqueClassOptions<Issue15DbContext>();

            using (var context = new Issue15DbContext(_options))
            {
                _connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
            }
        }

        [Fact]
        public void TestDifferentDefaultValues()
        {
            //SETUP
            using (var context = new Issue15DbContext(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                hasErrors.ShouldBeTrue();
                comparer.GetAllErrors.ShouldEqual(@"DIFFERENT: Message->Property 'BoolRequiredDefaultTrue', default value sql. Expected = True, found = 1
DIFFERENT: Message->Property 'EnumRequiredDefaultOne', default value sql. Expected = One, found = 1
DIFFERENT: Message->Property 'StringRequiredDefaultEmpty', default value sql. Expected = , found = N''
DIFFERENT: Message->Property 'StringRequiredDefaultSomething', default value sql. Expected = something, found = N'something'
DIFFERENT: Message->Property 'XmlRequiredDefaultEmpty', default value sql. Expected = , found = N''
DIFFERENT: Message->Property 'XmlRequiredDefaultSomething', default value sql. Expected = <something />, found = N'<something />'");
            }
        }

    }
}