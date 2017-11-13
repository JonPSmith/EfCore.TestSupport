// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Data.SqlClient;
using System.Linq;
using DataLayer.BookApp;
using Microsoft.EntityFrameworkCore;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
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
                .CreateUniqueClassOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
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