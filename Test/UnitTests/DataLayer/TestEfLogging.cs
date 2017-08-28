// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using DataLayer.EfClasses;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestEfLogging
    {


        [Fact]
        public void TestEfCoreLogging1()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<DbContextWithSchema>();
            using (var context = new DbContextWithSchema(options))
            {
                //ATTEMPT
                var logs = context.SetupLogging();
                context.Database.EnsureCreated();

                //VERIFY
                logs.Count.ShouldEqual(3);
            }
        }

        [Fact]
        public void TestEfCoreLogging2()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<DbContextWithSchema>();
            using (var context = new DbContextWithSchema(options))
            {
                //ATTEMPT
                var logs = context.SetupLogging();
                context.Database.EnsureCreated();

                //VERIFY
                logs.Count.ShouldEqual(3);
            }
        }

        [Fact]
        public void TestEfCoreLoggingWithMutipleDbContexts()
        {
            //SETUP
            List<string> logs1;
            var options1 = SqliteInMemory.CreateOptions<DbContextWithSchema>();
            using (var context = new DbContextWithSchema(options1))
            {
                //ATTEMPT
                logs1 = context.SetupLogging();
                context.Database.EnsureCreated();
            }
            var logs1Count = logs1.Count;
            var options2 = SqliteInMemory.CreateOptions<DbContextWithSchema>();
            using (var context = new DbContextWithSchema(options2))
            {
                //ATTEMPT
                var logs = context.SetupLogging();
                context.Database.EnsureCreated();

                //VERIFY
                logs.Count.ShouldBeInRange(1, 100);
                logs1.Count.ShouldNotEqual(logs1Count); //The second DbContext methods are also logged to the first logger
            }
        }
    }
}