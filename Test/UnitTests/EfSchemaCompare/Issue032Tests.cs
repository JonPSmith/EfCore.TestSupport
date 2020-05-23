// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.Issue32;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using TestSupport.EfHelpers;
using TestSupport.EfSchemeCompare;
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.EfSchemaCompare
{
    public class Issue032Tests
    {
        private DbContextOptions<Issue32Context> _options;

        public Issue032Tests()
        {
            var filepath = TestData.GetFilePath("AlterMyClassesToNotHaveAPrimaryKey.sql");
            _options = this.CreateUniqueClassOptions<Issue32Context>();
            using (var context = new Issue32Context(_options))
            {
                context.Database.EnsureCreated();
                context.ExecuteScriptFileInTransaction(filepath);
            }
        }


        [Fact]
        public void TestHandlesTableNoKey()
        {
            //SETUP
            using (var context = new Issue32Context(_options))
            {
                var comparer = new CompareEfSql();

                //ATTEMPT
                var hasErrors = comparer.CompareEfWithDb(context);

                //VERIFY
                hasErrors.ShouldBeTrue();
                comparer.GetAllErrors.ShouldEqual(@"DIFFERENT: MyClass->PrimaryKey 'PK_MyClasses', constraint name. Expected = PK_MyClasses, found = - no primary key -
NOT IN DATABASE: MyClass->Property 'Id', column name. Expected = Id
DIFFERENT: Entity 'MyClass', constraint name. Expected = PK_MyClasses, found = - no primary key -");
            }
        }
    }
}