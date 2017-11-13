// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer.BookApp;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestEfLogging
    {
        private readonly ITestOutputHelper _output; //#A

        public TestEfLogging(ITestOutputHelper output) //#B
        {
            _output = output;
        }

        [Fact]
        public void TestEfCoreLoggingExample()
        {
            //SETUP
            var options = SqliteInMemory
                .CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();
                var logs = context.SetupLogging(); //#C

                //ATTEMPT
                var books = context.Books.ToList(); //#D

                //VERIFY
                foreach (var log in logs.ToList()) //This stops the 'bleed' problem
                {                                     //#E
                    _output.WriteLine(log.ToString());//#E
                }                                     //#E
            }
        }
        /***********************************************************
        #A In xUnit, which runs in parallel, I need to use the ITestOutputHelper to output to the unit test runner
        #B The ITestOutputHelper is injected by the xUnit test runner
        #C Here I set up the logging, which returns a reference to a list of LogOutput classes. This contains separate properties for the LogLevel, EventId, Message and so on
        #D This is the query that I want to log
        #E This outputs the logged data
         * *********************************************************/

        [Fact]
        public void TestEfCoreLoggingStringWithBadValues()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                var logs = context.SetupLogging();
                context.Books.Count();
                context.Add(new Book {Title = "The person's boss said, \"What's that about?\""});
                context.SaveChanges();

                //VERIFY
                foreach (var log in logs.ToList()) //This stops the 'bleed' problem
                {
                    _output.WriteLine(log.ToString());
                }
            }
        }

        [Fact]
        public void TestEfCoreLogging1()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {
                //ATTEMPT
                var logs = context.SetupLogging();
                context.Database.EnsureCreated();

                //VERIFY
                foreach (var log in logs.ToList()) //This stops the 'bleed' problem
                {
                    _output.WriteLine(log.ToString());
                }
                logs.Count.ShouldBeInRange(11, 50);
            }
        }

        [Fact]
        public void TestEfCoreLogging2()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {
                //ATTEMPT
                var logs = context.SetupLogging();
                context.Database.EnsureCreated();

                //VERIFY
                logs.Count.ShouldBeInRange(11, 50);
            }
        }

        [Fact]
        public void TestEfCoreLoggingWithMutipleDbContexts()
        {
            //SETUP
            List<LogOutput> logs1;
            var options1 = SqliteInMemory.CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options1))
            {
                //ATTEMPT
                logs1 = context.SetupLogging();
                context.Database.EnsureCreated();
            }
            var logs1Count = logs1.Count;
            var options2 = SqliteInMemory.CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options2))
            {
                //ATTEMPT
                var logs = context.SetupLogging();
                context.Database.EnsureCreated();

                //VERIFY
                logs.Count.ShouldBeInRange(1, 100);
                logs1.Count.ShouldNotEqual(logs1Count); //The second DbContext methods are also logged to the first logger
            }
        }

        private class ClientSeverTestDto
        {
            public string ClientSideProp { get; set; }
        }

        [Fact]
        public void TestLogQueryClientEvaluationWarning()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {
                context.Database.EnsureCreated();
                var logs = context.SetupLogging();

                //ATTEMPT
                var books = context.Books.Select(x => new ClientSeverTestDto
                {
                    ClientSideProp = x.Price.ToString("C")
                }).OrderBy(x => x.ClientSideProp)
                .ToList();

                //VERIFY
                logs.ToList().Any(x => x.EventId.Name == RelationalEventId.QueryClientEvaluationWarning.Name).ShouldBeTrue();
            }
        }

        [Fact]
        public void TestQueryClientEvaluationThrowException()
        {
            //SETUP
            var options = SqliteInMemory
                .CreateOptions<EfCoreContext>(true); //#A
            using (var context = new EfCoreContext(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                var ex = Assert.Throws<InvalidOperationException>( //#B
                    () => context.Books.Select(x => //#C
                    new ClientSeverTestDto
                    {
                        ClientSideProp = x.Price.ToString("C")
                    }).OrderBy(x => x.ClientSideProp) //#D
                    .ToList());

                //VERIFY
                ex.Message.ShouldStartWith("Warning as error exception for warning 'Microsoft.EntityFrameworkCore.Query.QueryClientEvaluationWarning':");
            }
        }
        /*****************************************************************
        #A I set the optional throwOnClientServerWarning parameter to true, which means that an exception will be thrown by EF Core if a QueryClientEvaluationWarning is logged
        #B This is xUnit's assert for catching exception
        #C This is the query that will log a QueryClientEvaluationWarning
        #D This is the part of the query which causes the QueryClientEvaluationWarning to be logged.
         * **************************************************************/
    }
}