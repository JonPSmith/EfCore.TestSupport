// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.BookApp;
using DataLayer.EfCode.BookApp;
using DataLayer.SpecialisedEntities;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataResetter
{
    public class TestResetKeysSingleEntity 
    {
        private readonly ITestOutputHelper _output;

        public TestResetKeysSingleEntity(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestResetKeysSingleEntityPkOnly()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();
                var entity = context.Books.First();

                //ATTEMPT
                var resetter = new DataResetter(context);
                resetter.ResetKeysSingleEntity(entity);

                //VERIFY 
                entity.BookId.ShouldEqual(0);
            }
        }

        [Fact]
        public void TestResetKeysSingleEntityPkAndForeignKey()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();
                var entity = context.Set<Review>().First();

                //ATTEMPT
                var resetter = new DataResetter(context);
                resetter.ResetKeysSingleEntity(entity);

                //VERIFY 
                entity.ReviewId.ShouldEqual(0);
                entity.BookId.ShouldEqual(0);
            }
        }

        [Fact]
        public void TestResetKeysSingleEntityPrivateSetter()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<SpecializedDbContext>();
            using (var context = new SpecializedDbContext(options))
            {
                var entity = new AllTypesEntity();
                entity.SetId(123);

                //ATTEMPT
                var resetter = new DataResetter(context);
                resetter.ResetKeysSingleEntity(entity);

                //VERIFY 
                entity.Id.ShouldEqual(0);
            }
        }

        [Fact]
        public void TestResetKeysSingleEntityAlternative()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<OwnedWithKeyDbContext>();
            using (var context = new OwnedWithKeyDbContext(options))
            {
                var entity = new User
                {
                    UserId = 123,
                    Email = "Hello"
                };

                //ATTEMPT
                var resetter = new DataResetter(context);
                resetter.ResetKeysSingleEntity(entity);

                //VERIFY 
                entity.UserId.ShouldEqual(0);
                entity.Email.ShouldBeNull();
            }
        }

        [Fact]
        public void TestResetKeysSingleEntityAlternativeNotReset()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<OwnedWithKeyDbContext>();
            using (var context = new OwnedWithKeyDbContext(options))
            {
                var entity = new User
                {
                    UserId = 123,
                    Email = "Hello"
                };

                //ATTEMPT
                var config = new DataResetterConfig {DoNotResetAlternativeKey = true};
                var resetter = new DataResetter(context, config);
                resetter.ResetKeysSingleEntity(entity);

                //VERIFY 
                entity.UserId.ShouldEqual(0);
                entity.Email.ShouldEqual("Hello");
            }
        }

        [Fact]
        public void TestResetKeysSingleEntityNonEntityClassBad()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                var entity = new User();

                //ATTEMPT
                var resetter = new DataResetter(context);
                var ex = Assert.Throws<InvalidOperationException>(() => resetter.ResetKeysSingleEntity(entity));

                //VERIFY 
                ex.Message.ShouldEqual("The class User is not a class that the provided DbContext knows about.");
            }
        }

    }
}