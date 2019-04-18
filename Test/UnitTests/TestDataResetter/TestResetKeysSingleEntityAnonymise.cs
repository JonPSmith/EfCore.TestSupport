// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.BookApp;
using DataLayer.EfCode.BookApp;
using RandomNameGeneratorLibrary;
using TestSupport.EfHelpers;
using TestSupport.SeedDatabase;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataResetter
{
    public class TestResetKeysSingleEntityAnonymise
    {
        private readonly ITestOutputHelper _output;

        public TestResetKeysSingleEntityAnonymise(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestResetKeysSingleEntityAnonymiseName()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                var entity = new Author {Name = "Test"};

                //ATTEMPT
                var config = new DataResetterConfig();
                config.AddToAnonymiseList<Author>(x => x.Name, "Name");
                var resetter = new DataResetter(context, config);
                resetter.ResetKeysSingleEntity(entity);

                //VERIFY 
                entity.Name.ShouldNotEqual("Test");
            }
        }

        [Fact]
        public void TestResetKeysSingleEntityAnonymiseNameAsEmail()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                var entity = new Author { Name = "Test" };

                //ATTEMPT
                var config = new DataResetterConfig();
                config.AddToAnonymiseList<Author>(x => x.Name, "Email");
                var resetter = new DataResetter(context, config);
                resetter.ResetKeysSingleEntity(entity);

                //VERIFY 
                entity.Name.ShouldEndWith(DataResetterConfig.EmailSuffix);
            }
        }

        [Fact]
        public void TestResetKeysSingleEntityAnonymiseNameMaxLength()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                var entity = new Author { Name = "Test" };

                //ATTEMPT
                var config = new DataResetterConfig();
                config.AddToAnonymiseList<Author>(x => x.Name, "Name:Max=5");
                var resetter = new DataResetter(context, config);
                resetter.ResetKeysSingleEntity(entity);

                //VERIFY 
                entity.Name.ShouldNotEqual("Test");
                entity.Name.Length.ShouldEqual(5);
            }
        }

        [Fact]
        public void TestResetKeysSingleEntityAnonymiseNameAsEmailMaxLength()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                var entity = new Author { Name = "Test" };

                //ATTEMPT
                var config = new DataResetterConfig();
                config.AddToAnonymiseList<Author>(x => x.Name, "Email:Max=10");
                var resetter = new DataResetter(context, config);
                resetter.ResetKeysSingleEntity(entity);

                //VERIFY 
                entity.Name.ShouldEndWith(DataResetterConfig.EmailSuffix);
                entity.Name.Length.ShouldEqual(10);
            }
        }

        [Fact]
        public void TestResetKeysSingleEntityAnonymiseNameMinLength()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                var entity = new Author { Name = "Test" };

                //ATTEMPT
                var config = new DataResetterConfig();
                config.AddToAnonymiseList<Author>(x => x.Name, "Name:Min=100");
                var resetter = new DataResetter(context, config);
                resetter.ResetKeysSingleEntity(entity);

                //VERIFY 
                entity.Name.Length.ShouldBeInRange(100, 140);
            }
        }

        [Fact]
        public void TestResetKeysSingleEntityAnonymiseOwnAnonymiser()
        {
            //SETUP
            string MyAnonymiser(AnonymiserData data, object objectInstance)
            {
                return "My Replacement text";
            }

            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                var entity = new Author { Name = "Test" };

                //ATTEMPT
                var config = new DataResetterConfig
                {
                    AnonymiserFunc = MyAnonymiser
                };
                config.AddToAnonymiseList<Author>(x => x.Name, "Name");
                var resetter = new DataResetter(context, config);
                resetter.ResetKeysSingleEntity(entity);

                //VERIFY 
                entity.Name.ShouldEqual("My Replacement text");
            }
        }



    }
}