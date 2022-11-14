// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer.BookApp.EfCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestOptionsWithLogTo
    {
        private readonly ITestOutputHelper _output;

        public TestOptionsWithLogTo(ITestOutputHelper output) 
        {
            _output = output;
        }

        [Fact]
        public void TestEfCoreLoggingExampleOfOutputToConsole()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptionsWithLogTo<BookContext>(_output.WriteLine);
            using var context = new BookContext(options);
            context.Database.EnsureCreated();
            context.SeedDatabaseFourBooks();
                
            //ATTEMPT
            var books = context.Books.ToList(); 

            //VERIFY                                    
        }

        [Fact]
        public void TestEfCoreLoggingCheckSqlOutput()
        {
            //SETUP
            var logs = new List<string>();
            var options = SqliteInMemory.CreateOptionsWithLogTo<BookContext>(log => logs.Add(log));
            using var context = new BookContext(options);
            context.Database.EnsureCreated();
            context.SeedDatabaseFourBooks();

            //ATTEMPT 
            var book = context.Books.Single(x => x.Reviews.Count() > 1);

            //VERIFY
            logs.Last().ShouldStartWith("Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']");
        }

        [Fact]
        public void TestEfCoreLoggingCheckSqlOutputShowLog()
        {
            //SETUP
            var logs = new List<string>();
            var logToOptions = new LogToOptions
            {
                ShowLog = false
            };
            var options = SqliteInMemory.CreateOptionsWithLogTo<BookContext>(log => logs.Add(log), logToOptions);
            using var context = new BookContext(options);
            context.Database.EnsureCreated();
            context.SeedDatabaseFourBooks();

            //ATTEMPT 
            logToOptions.ShowLog = true;
            var book = context.Books.Single(x => x.Reviews.Count() > 1);

            //VERIFY
            logs.Count.ShouldEqual(1);
            _output.WriteLine(logs.Single());
            logs.Single().ShouldStartWith("Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']");
        }

        [Fact]
        public void TestEfCoreLoggingCheckOnlyShowTheseCategories()
        {
            //SETUP
            var logs = new List<string>();
            var logToOptions = new LogToOptions
            {
                OnlyShowTheseCategories = new[] {DbLoggerCategory.Database.Command.Name}
            };
            var options = SqliteInMemory.CreateOptionsWithLogTo<BookContext>(log => logs.Add(log), logToOptions);
            using var context = new BookContext(options);
            context.Database.EnsureCreated();
            context.SeedDatabaseFourBooks();

            //ATTEMPT 
            var books = context.Books.Select(x => x.BookId).ToList();

            //VERIFY
            logs.All(x => x.StartsWith("Executed DbCommand")).ShouldBeTrue();
        }

        [Fact]
        public void TestEfCoreLoggingCheckOnlyShowTheseEvents()
        {
            //SETUP
            var logs = new List<string>();
            var logToOptions = new LogToOptions
            {
                OnlyShowTheseEvents = new[] { CoreEventId.ContextInitialized, CoreEventId.SensitiveDataLoggingEnabledWarning }
            };
            var options = SqliteInMemory.CreateOptionsWithLogTo<BookContext>(log => logs.Add(log), logToOptions);
            using var context = new BookContext(options);
            context.Database.EnsureCreated();
            context.SeedDatabaseFourBooks();

            //ATTEMPT 
            var books = context.Books.Select(x => x.BookId).ToList();

            //VERIFY
            logs.Count.ShouldEqual(1);
            logs.Single().ShouldStartWith("Sensitive data logging is enabled.");
        }

        [Fact]
        public void TestEfCoreLoggingCheckFilterFunction()
        {
            bool MyFilterFunction(EventId eventId, LogLevel logLevel)
            {
                return eventId.Name == RelationalEventId.CommandExecuted.Name && logLevel == LogLevel.Information;
            }

            //SETUP
            var logs = new List<string>();
            var logToOptions = new LogToOptions
            {
                FilterFunction = MyFilterFunction
            };
            var options = SqliteInMemory.CreateOptionsWithLogTo<BookContext>(log => logs.Add(log), logToOptions);
            using var context = new BookContext(options);
            context.Database.EnsureCreated();
            context.SeedDatabaseFourBooks();

            //ATTEMPT 
            var books = context.Books.Select(x => x.BookId).ToList();

            //VERIFY
            logs.All(l => l.StartsWith("Executed DbCommand ")).ShouldBeTrue();
        }

        [Fact]
        public void TestEfCoreLoggingLogToOptionBad()
        {
            //SETUP
            var logs = new List<string>();
            var logToOptions = new LogToOptions
            {
                OnlyShowTheseEvents = new[] { CoreEventId.ContextInitialized },
                OnlyShowTheseCategories = new[] { DbLoggerCategory.Database.Command.Name }
            };

            //ATTEMPT 
            var ex = Assert.Throws<NotSupportedException>(() =>
                SqliteInMemory.CreateOptionsWithLogTo<BookContext>(log => logs.Add(log), logToOptions));

            //VERIFY
            ex.Message.ShouldEqual("You can't define OnlyShowTheseCategories and OnlyShowTheseEvents at the same time.");
        }

        [Fact]
        public void TestEfCoreLoggingCheckLoggerOptionsDefaultWithUtcTime()
        {
            //SETUP
            var logs = new List<string>();
            var logToOptions = new LogToOptions
            {
                LoggerOptions = DbContextLoggerOptions.DefaultWithUtcTime
            };
            var options = SqliteInMemory.CreateOptionsWithLogTo<BookContext>(log => logs.Add(log), logToOptions);
            using var context = new BookContext(options);
            context.Database.EnsureCreated();
            context.SeedDatabaseFourBooks();

            //ATTEMPT 
            var books = context.Books.Select(x => x.BookId).ToList();

            //VERIFY
            logs.All(x => x.StartsWith("warn:") || x.StartsWith("info:")).ShouldBeTrue();
        }

        [Fact]
        public void TestEfCoreLoggingCheckLoggerOptionsSingleLine()
        {
            //SETUP
            var logs = new List<string>();
            var logToOptions = new LogToOptions
            {
                LoggerOptions = DbContextLoggerOptions.Id
            };
            var options = SqliteInMemory.CreateOptionsWithLogTo<BookContext>(log => logs.Add(log), logToOptions);
            using var context = new BookContext(options);
            context.Database.EnsureCreated();
            context.SeedDatabaseFourBooks();

            //ATTEMPT 
            var books = context.Books.Select(x => x.BookId).ToList();

            //VERIFY
            logs.All(x => x.StartsWith("RelationalEventId.CommandExecuted") || x.StartsWith("CoreEventId.")).ShouldBeTrue();
        }


        [Fact]
        public void TestCreateUniqueClassOptionsWithLogTo()
        {
            //SETUP
            var logs = new List<string>();
            var options = this.CreateUniqueClassOptionsWithLogTo<BookContext>(log => logs.Add(log));
            using var context = new BookContext(options);
            context.Database.EnsureCreated();
            context.SeedDatabaseFourBooks();

            //ATTEMPT 
            var book = context.Books.Where(x => x.Reviews.Count() > 1).Select(x => x.BookId).First();

            //VERIFY
            _output.WriteLine(logs.Last());
            var lines = logs.Last().Split('\n').Select(x => x.Trim()).ToArray();
            lines[1].ShouldEqual("SELECT TOP(1) [b].[BookId]");
        }

        [Fact]
        public void TestCreateUniqueMethodOptionsWithLogTo()
        {
            //SETUP
            var logs = new List<string>();
            var options = this.CreateUniqueMethodOptionsWithLogTo<BookContext>(log => logs.Add(log));
            using var context = new BookContext(options);
            context.Database.EnsureCreated();
            context.SeedDatabaseFourBooks();

            //ATTEMPT 
            var book = context.Books.Where(x => x.Reviews.Count() > 1).Select(x => x.BookId).First();

            //VERIFY
            var lines = logs.Last().Split('\n').Select(x => x.Trim()).ToArray();
            lines[1].ShouldEqual("SELECT TOP(1) [b].[BookId]");
        }
    }
}