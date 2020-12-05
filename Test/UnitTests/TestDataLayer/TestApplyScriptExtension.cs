// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.BookApp.EfCode;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestApplyScriptExtension

    {
        private readonly ITestOutputHelper _output;

        public TestApplyScriptExtension(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestApplyScriptOneCommandToDatabaseOk()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>();
            var filepath = TestData.GetFilePath("Script01 - Add row to Authors table.sql");
            using (var context = new BookContext(options))
            {
                context.Database.EnsureClean();

                //ATTEMPT
                context.ExecuteScriptFileInTransaction(filepath);

                //VERIFY
                context.Authors.Count().ShouldEqual(1);
            }
        }

        [Fact]
        public void TestApplyScriptTwoCommandsToDatabaseOk()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<BookContext>();
            var filepath = TestData.GetFilePath("Script02 - Add two rows to Authors table.sql");
            using (var context = new BookContext(options))
            {
                context.Database.EnsureClean();

                //ATTEMPT
                context.ExecuteScriptFileInTransaction(filepath);

                //VERIFY
                context.Authors.Count().ShouldEqual(2);
            }
        }
    }
}