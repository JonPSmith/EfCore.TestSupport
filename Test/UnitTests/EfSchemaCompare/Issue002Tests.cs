// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using DataLayer.Issue2;
using TestSupport.EfHelpers;
using TestSupport.EfSchemeCompare;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.EfSchemaCompare
{
    public class Issue002Tests
    {
        [Fact]
        public void CompareIssue2()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<Issue2DbContext>();
            using (var context = new Issue2DbContext(options))
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

    }
}