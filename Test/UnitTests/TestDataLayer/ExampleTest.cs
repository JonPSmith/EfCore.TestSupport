// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.BookApp;
using DataLayer.EfCode.BookApp;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class ExampleTest 
    {
        private readonly ITestOutputHelper _output;

        public ExampleTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestExample()
        {
            //SETUP
            var options = this
                .CreateUniqueClassOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                context.CreateEmptyViaWipe();
                var logs = context.SetupLogging();

                //ATTEMPT
                context.Add(new Book {Title = "New Book"});
                context.SaveChanges();

                //VERIFY
                context.Books.Count().ShouldEqual(1);
                foreach (var log in logs.ToList())
                {                                    
                    _output.WriteLine(log.ToString());
                }                                     
            }
        }

    }
}