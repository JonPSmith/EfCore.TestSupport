// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using DataLayer.Issue15;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;
using TestSupport.EfSchemeCompare;
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
        public void TestDifferentDefaultValuesReturnErrors()
        {
            //SETUP
            using (var context = new Issue15DbContext(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                //hasErrors.ShouldBeFalse(comparer.GetAllErrors);
                hasErrors.ShouldBeTrue();
#if NETCOREAPP2_1
                comparer.GetAllErrors.ShouldEqual(@"DIFFERENT: Message->Property 'BoolRequiredDefaultFalse', default value sql. Expected = False, found = <null>
DIFFERENT: Message->Property 'BoolRequiredDefaultFalse', value generated. Expected = OnAdd, found = Never
DIFFERENT: Message->Property 'BoolRequiredDefaultTrue', default value sql. Expected = True, found = 1
DIFFERENT: Message->Property 'EnumRequiredDefaultOne', default value sql. Expected = One, found = 1
DIFFERENT: Message->Property 'EnumRequiredDefaultZero', default value sql. Expected = Zero, found = <null>
DIFFERENT: Message->Property 'EnumRequiredDefaultZero', value generated. Expected = OnAdd, found = Never
DIFFERENT: Message->Property 'IntRequiredDefault0', default value sql. Expected = 0, found = <null>
DIFFERENT: Message->Property 'IntRequiredDefault0', value generated. Expected = OnAdd, found = Never
DIFFERENT: Message->Property 'StringRequiredDefaultEmpty', default value sql. Expected = , found = N''
DIFFERENT: Message->Property 'StringRequiredDefaultSomething', default value sql. Expected = something, found = N'something'
DIFFERENT: Message->Property 'XmlRequiredDefaultEmpty', default value sql. Expected = , found = N''
DIFFERENT: Message->Property 'XmlRequiredDefaultSomething', default value sql. Expected = <something />, found = N'<something />'");
#elif NETCOREAPP3_0
                comparer.GetAllErrors.ShouldEqual(@"DIFFERENT: Message->Property 'BoolRequiredDefaultFalse', default value sql. Expected = False, found = CONVERT([bit],(0))
DIFFERENT: Message->Property 'BoolRequiredDefaultTrue', default value sql. Expected = True, found = CONVERT([bit],(1))
DIFFERENT: Message->Property 'EnumRequiredDefaultOne', default value sql. Expected = One, found = 1
DIFFERENT: Message->Property 'EnumRequiredDefaultZero', default value sql. Expected = Zero, found = <null>
DIFFERENT: Message->Property 'EnumRequiredDefaultZero', value generated. Expected = OnAdd, found = Never
DIFFERENT: Message->Property 'IntRequiredDefault0', default value sql. Expected = 0, found = <null>
DIFFERENT: Message->Property 'IntRequiredDefault0', value generated. Expected = OnAdd, found = Never
DIFFERENT: Message->Property 'StringRequiredDefaultEmpty', default value sql. Expected = , found = N''
DIFFERENT: Message->Property 'StringRequiredDefaultSomething', default value sql. Expected = something, found = N'something'
DIFFERENT: Message->Property 'XmlRequiredDefaultEmpty', default value sql. Expected = , found = N''
DIFFERENT: Message->Property 'XmlRequiredDefaultSomething', default value sql. Expected = <something />, found = N'<something />'");
#endif

            }
        }

        [Fact]
        public void TestDifferentDefaultValuesSuppressDefaultValueErrors()
        {
            //SETUP
            using (var context = new Issue15DbContext(_options))
            {
                var config = new CompareEfSqlConfig();
                config.AddIgnoreCompareLog(new CompareLog(CompareType.Property, CompareState.Different, null, CompareAttributes.DefaultValueSql));
                config.AddIgnoreCompareLog(new CompareLog(CompareType.Property, CompareState.Different, null, CompareAttributes.ValueGenerated, "OnAdd", "Never"));
                var comparer = new CompareEfSql(config);

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                hasErrors.ShouldBeFalse(comparer.GetAllErrors);
            }
        }

    }
}