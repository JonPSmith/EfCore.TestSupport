// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.DiffConfig;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestSqliteLimitations
    {
        private readonly ITestOutputHelper _output;

        public TestSqliteLimitations(ITestOutputHelper output)
        {
            _output = output;
        }

        //see https://docs.microsoft.com/en-us/ef/core/modeling/dynamic-model
        private class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
        {
            public object Create(DbContext context)
                => context is DiffConfigDbContext dynamicContext
                    ? (context.GetType(), dynamicContext.Config)
                    : (object)context.GetType();
        }

        [Fact]
        public void TestSqlLiteAddSchemaIgnored()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptionsWithLogTo<DiffConfigDbContext>(_output.WriteLine, null,
                builder => builder.ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>());

            using var context = new DiffConfigDbContext(options, DiffConfigs.AddSchema);

            //ATTEMPT
            context.Database.EnsureCreated();

            //VERIFY
        }

        [Fact]
        public void TestSqlLiteAddSequenceFAILS()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptionsWithLogTo<DiffConfigDbContext>(_output.WriteLine, null,
                builder => builder.ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>());

            using var context = new DiffConfigDbContext(options, DiffConfigs.AddSequence);

            //ATTEMPT
            var ex = Assert.Throws<NotSupportedException>(() => context.Database.EnsureCreated());

            //VERIFY
        }


        [Fact]
        public void TestSqlLiteComputedColWorks()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptionsWithLogTo<DiffConfigDbContext>(_output.WriteLine, null,
                builder => builder.ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>());

            using var context = new DiffConfigDbContext(options, DiffConfigs.SetComputedCol);

            //ATTEMPT
            context.Database.EnsureCreated();
            context.Add(new MyEntity {MyInt = 123});
            context.SaveChanges();

            //VERIFY
            context.ChangeTracker.Clear();
            context.MyEntities.Single().MyString.ShouldEqual("123");
        }

        [Fact]
        public void TestSqlLiteDefaultColValueWorks()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptionsWithLogTo<DiffConfigDbContext>(_output.WriteLine, null,
                builder => builder.ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>());

            using var context = new DiffConfigDbContext(options, DiffConfigs.SetDefaultCol);

            //ATTEMPT
            context.Database.EnsureCreated();

            //VERIFY
        }


        [Fact]
        public void TestSqlLiteDefaultUserDefinedFunctionsFAILsOnRun()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptionsWithLogTo<DiffConfigDbContext>(_output.WriteLine, null,
                builder => builder.ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>());

            using var context = new DiffConfigDbContext(options, DiffConfigs.Nothing);
            context.Database.EnsureCreated();

            //ATTEMPT
            var filepath = TestData.GetFilePath("AddUserDefinedFunctions.sql");
            var ex = Assert.Throws<SqliteException>(() => context.ExecuteScriptFileInTransaction(filepath));

            //VERIFY
            ex.Message.ShouldEqual("SQLite Error 1: 'near \"IF\": syntax error'.");
        }
    }
}