// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using DataLayer.BookApp.EfCode;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestMyLoggerProviderActionOut
    {
        private readonly ITestOutputHelper _output;

        public TestMyLoggerProviderActionOut(ITestOutputHelper output) 
        {
            _output = output;
        }

        class MyService
        {
            private readonly ILogger _logger;

            public MyService(ILogger<MyService> logger)
            {
                _logger = logger;
            }

            public void AddInfoLog()
            {
                _logger.LogInformation("This is a log");
            }
        }

        [Fact]
        public void TestDependencyInjectionFailIfNoLogger()
        {
            //SETUP
            var services = new ServiceCollection();
            services.AddSingleton<MyService>();

            //ATTEMPT
            var serviceProvider = services.BuildServiceProvider();
            var ex = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService<MyService>());

            //VERIFY
            ex.Message.ShouldStartWith("Unable to resolve service");
        }

        [Fact]
        public void TestAddLoggerInDependencyInjection()
        {
            //SETUP
            var services = new ServiceCollection();
            services.AddTransient<MyService>();
            services.AddLogging();

            //ATTEMPT
            var serviceProvider = services.BuildServiceProvider();
            var myServiceDI = serviceProvider.GetService<MyService>();

            //VERIFY
            myServiceDI.ShouldNotBeNull();
            myServiceDI.AddInfoLog();
        }

        [Fact]
        public void TestAddingLoggerInDependencyInjection()
        {
            //SETUP
            var logs = new List<LogOutput>();
            var services = new ServiceCollection();
            services.AddTransient<MyService>();
            services.AddSingleton<ILogger<MyService>>(x =>
                new LoggerFactory(
                        new[] { new MyLoggerProviderActionOut(l => logs.Add(l)) })
                    .CreateLogger<MyService>());

            //ATTEMPT
            var serviceProvider = services.BuildServiceProvider();
            var myServiceDI = serviceProvider.GetRequiredService<MyService>();

            //VERIFY
            logs.Count.ShouldEqual(0);
            myServiceDI.AddInfoLog();
            logs.Count.ShouldEqual(1);
            _output.WriteLine(logs.Single().ToString());
        }

        class MyOtherService
        {
            private readonly ILogger _logger;

            public MyOtherService(ILogger<MyOtherService> logger)
            {
                _logger = logger;
            }

            public void AddInfoLog()
            {
                _logger.LogInformation("This is a log");
            }
        }

        [Fact]
        public void TestAddingLoggerInDependencyInjectionWithAddLogging()
        {
            //SETUP
            var logs = new List<LogOutput>();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddTransient<MyService>();
            services.AddTransient<MyOtherService>();
            services.AddSingleton<ILogger<MyService>>(x =>
                new LoggerFactory(
                        new[] { new MyLoggerProviderActionOut(l => logs.Add(l)) })
                    .CreateLogger<MyService>());

            //ATTEMPT
            var serviceProvider = services.BuildServiceProvider();
            var myServiceDI = serviceProvider.GetRequiredService<MyService>();
            var myOtherService = serviceProvider.GetRequiredService<MyOtherService>();

            //VERIFY
            logs.Count.ShouldEqual(0);
            myServiceDI.AddInfoLog();
            logs.Count.ShouldEqual(1);
            _output.WriteLine(logs.Single().ToString());
            myOtherService.AddInfoLog();
        }

        [Fact]
        public void TestDecodeMessageNotSensitiveLogging()
        {
            //SETUP
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var logs = new List<LogOutput>();
            var options = new DbContextOptionsBuilder<BookContext>()
                .UseLoggerFactory(new LoggerFactory(new[] { new MyLoggerProviderActionOut(l => logs.Add(l))}))
                .UseSqlite(connection)
                .EnableSensitiveDataLogging()
                .Options;
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                //ATTEMPT
                var id = context.Books.First().BookId;
                var book = context.Books.Single(x => x.BookId == id);
                var decoded = logs.Last().DecodeMessage();

                //VERIFY
                var sqlCommand = decoded.Split('\n').Skip(1).Select(x => x.Trim()).ToArray();
                sqlCommand[0].ShouldEqual(
                    "SELECT \"b\".\"BookId\", \"b\".\"Description\", \"b\".\"ImageUrl\", \"b\".\"Price\", \"b\".\"PublishedOn\", \"b\".\"Publisher\", \"b\".\"SoftDeleted\", \"b\".\"Title\"");
                sqlCommand[1].ShouldEqual("FROM \"Books\" AS \"b\"");
                sqlCommand[2].ShouldEqual("WHERE NOT (\"b\".\"SoftDeleted\") AND (\"b\".\"BookId\" = '1')");
            }
        }
    }
}