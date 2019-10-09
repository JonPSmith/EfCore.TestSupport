// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using DataLayer.BookApp;
using DataLayer.EfCode.BookApp;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Test.Helpers;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestEfLoggingNewApproach
    {
        private readonly ITestOutputHelper _output; 

        public TestEfLoggingNewApproach(ITestOutputHelper output) 
        {
            _output = output;
        }

        [Fact]
        public void TestEfCoreLoggingViaBuilder()
        {
            //SETUP
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var logs = new List<LogOutput>();

            var options = new DbContextOptionsBuilder<BookContext>()
                .UseLoggerFactory(new LoggerFactory(new[] { new MyLoggerProviderActionOut(log => logs.Add(log)) }))
                .UseSqlite(connection)
                .Options;
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                //ATTEMPT
                logs.Clear();
                var books = context.Books.ToList();

                //VERIFY
#if NETCOREAPP2_1
                logs.Single().ToString().ShouldStartWith("Information,CommandExecuted: Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']");
                logs.Single().Message.ShouldStartWith("Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']");
#elif NETCOREAPP3_0
                logs.Single().ToString().ShouldStartWith("Information,CommandExecuting: Executing DbCommand [Parameters=[], CommandType='Text', CommandTimeout='30']");
                logs.Single().Message.ShouldStartWith("Executing DbCommand [Parameters=[], CommandType='Text', CommandTimeout='30']");
#endif
            }
        }

        [Fact]
        public void TestEfCoreLoggingWithMultipleDbContextsViaBuilder()
        {
            //SETUP
            var logs1 = new List<LogOutput>();
            var connection1 = new SqliteConnection("DataSource=:memory:");
            connection1.Open();

            var options1 = new DbContextOptionsBuilder<BookContext>()
                .UseLoggerFactory(new LoggerFactory(new[] { new MyLoggerProviderActionOut(log => logs1.Add(log)) }))
                .UseSqlite(connection1)
                .Options;
            using (var context = new BookContext(options1))
            {
                //ATTEMPT
                context.Database.EnsureCreated();
            }
            var logs1Count = logs1.Count;

            var logs2 = new List<LogOutput>();
            var connection2 = new SqliteConnection("DataSource=:memory:");
            connection1.Open();

            var options2 = new DbContextOptionsBuilder<BookContext>()
                .UseLoggerFactory(new LoggerFactory(new[] { new MyLoggerProviderActionOut(log => logs2.Add(log))}))
                .UseSqlite(connection2)
                .Options;
            using (var context = new BookContext(options2))
            {
                //ATTEMPT
                context.Database.EnsureCreated();

                //VERIFY
                logs2.Count.ShouldBeInRange(1, 100);
                logs1.Count.ShouldEqual(logs1Count); 
            }
        }


        [Fact]
        public void TestEfCoreLoggingExampleOfOutputToConsole()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptionsWithLogging<BookContext>(l => _output.WriteLine(l.Message));
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();
                 

                //ATTEMPT
                var books = context.Books.ToList(); 

                //VERIFY                                    
            }
        }

        [Fact]
        public void TestEfCoreLoggingCheckSqlOutput()
        {
            //SETUP
            var logs = new List<LogOutput>();
            var options = SqliteInMemory.CreateOptionsWithLogging<BookContext>(log => logs.Add(log));
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT 
                context.Books.Count();

                //VERIFY
#if NETCOREAPP2_1
                logs.Last().Message.ShouldEndWith("SELECT COUNT(*)\r\nFROM \"Books\" AS \"p\"\r\nWHERE \"p\".\"SoftDeleted\" = 0");
#elif NETCOREAPP3_0
                logs.Last().Message.ShouldEndWith("SELECT COUNT(*)\r\nFROM \"Books\" AS \"b\"\r\nWHERE NOT (\"b\".\"SoftDeleted\")");
#endif
            }
        }


        [Fact]
        public void TestEfCoreLoggingStringWithBadValues()
        {
            //SETUP
            var logs = new List<LogOutput>();
            var options = SqliteInMemory.CreateOptionsWithLogging<BookContext>(log => logs.Add(log));
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT            
                context.Books.Count();
                context.Add(new Book {Title = "The person's boss said, \"What's that about?\""});
                context.SaveChanges();

                //VERIFY
                foreach (var log in logs.ToList())
                {
                    _output.WriteLine(log.ToString());
                }
            }
        }

        [Fact]
        public void TestEfCoreLogging1()
        {
            //SETUP
            var logs = new List<LogOutput>();
            var options = SqliteInMemory.CreateOptionsWithLogging<BookContext>(log => logs.Add(log));
            using (var context = new BookContext(options))
            {
                //ATTEMPT
                
                context.Database.EnsureCreated();

                //VERIFY
                foreach (var log in logs)
                {
                    _output.WriteLine(log.ToString());
                }
                logs.Count.ShouldBeInRange(11, 50);
            }
        }

        [Fact]
        public void TestEfCoreLoggingWithMultipleDbContexts()
        {
            //SETUP
            var logs1 = new List<LogOutput>();
            var options1 = SqliteInMemory.CreateOptionsWithLogging<BookContext>(log => logs1.Add(log));
            var logs2 = new List<LogOutput>();
            var options2 = SqliteInMemory.CreateOptionsWithLogging<BookContext>(log => logs2.Add(log));
            using (var context1 = new BookContext(options1))
            using (var context2 = new BookContext(options2))
            {
                //ATTEMPT
                context1.Database.EnsureCreated();
                var logs1Count = logs1.Count;
                
                context2.Database.EnsureCreated();

                //VERIFY
                logs1.Count.ShouldEqual(logs1Count);
#if NETCOREAPP2_1
                logs2.Count.ShouldEqual(logs1Count);
#elif NETCOREAPP3_0
                logs2.Count.ShouldEqual(logs1Count - 1);
#endif
            }
        }

        private class ClientSeverTestDto
        {
            public string ClientSideProp { get; set; }
        }

#if NETCOREAPP2_1
        [Fact]
        public void TestLogQueryClientEvaluationWarning()
        {
            //SETUP
            var logs = new List<LogOutput>();
            var options = SqliteInMemory.CreateOptionsWithLogging<BookContext>(log => logs.Add(log), LogLevel.Warning, false);
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                

                //ATTEMPT
                var books = context.Books.Select(x => new ClientSeverTestDto
                {
                    ClientSideProp = x.Price.ToString("C")
                }).OrderBy(x => x.ClientSideProp)
                .ToList();

                //VERIFY
                logs.Any(x => x.EventId.Name == RelationalEventId.QueryClientEvaluationWarning.Name).ShouldBeTrue();
                logs.All(x => x.LogLevel >= LogLevel.Warning ).ShouldBeTrue();
            }
        }
#endif

        [RunnableInDebugOnly]
        public void CaptureSqlEfCoreCreatesDatabaseToConsole()
        {
            //SETUP
            var options = this.CreateUniqueClassOptionsWithLogging<BookContext>(log => _output.WriteLine(log.Message));
            using (var context = new BookContext(options))
            {

                //ATTEMPT
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                //VERIFY
            }
        }
    }
}