// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfCode.BookApp;
using DataLayer.Issue2;
using DataLayer.SpecialisedEntities;
using Microsoft.EntityFrameworkCore;
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

        [Fact]
        public void CompareKeys()
        {
            //SETUP
            var options1 = this.CreateUniqueClassOptions<Issue2DbContext>();
            var options2 = this.CreateUniqueMethodOptions<BookContext>();
            using (var context1 = new Issue2DbContext(options1))
            using (var context2 = new BookContext(options2))
            {

                //ATTEMPT
                var userSetKey = context1.Model.GetEntityTypes().First().GetProperties().First();
                var dbSetKey = context2.Model.GetEntityTypes().First().GetProperties().First();

                //VERIFY
            }
        }
    }
}