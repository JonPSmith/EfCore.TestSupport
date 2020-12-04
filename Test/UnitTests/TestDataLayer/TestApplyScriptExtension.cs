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
                context.CreateEmptyViaWipe();

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
                context.CreateEmptyViaWipe();

                //ATTEMPT
                context.ExecuteScriptFileInTransaction(filepath);

                //VERIFY
                context.Authors.Count().ShouldEqual(2);
            }
        }

        [Fact]
        public void TestApplyScriptExampleOk()
        {
            //SETUP
            var options = this
                .CreateUniqueClassOptions<BookContext>();
            var filepath = TestData      //#A
                .GetFilePath(           //#A
                "AddUserDefinedFunctions.sql"); //#A
            using (var context = new BookContext(options))
            {
                if (context.CreateEmptyViaWipe()) //#B
                {
                    context                             //#C
                        .ExecuteScriptFileInTransaction(//#C
                        filepath);                      //#C
                }

            }
        }
        /*********************************************************
        #A I get the filepath of the SQL script file via my GetTestDataFilePath method
        #B I use my CreateEmptyViaWipe to ensure the database is empty. This returns true if a new database was created
        #C A new database was created, so I need to apply my script to the database using the ExecuteScriptFileInTransaction method
         * *******************************************************/
    }
}