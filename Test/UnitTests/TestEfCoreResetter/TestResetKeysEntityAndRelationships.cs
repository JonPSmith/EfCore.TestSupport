// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfCode.BookApp;
using Microsoft.EntityFrameworkCore;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestEfCoreResetter
{
    public class TestResetKeysEntityAndRelationships 
    {
        private readonly ITestOutputHelper _output;

        public TestResetKeysEntityAndRelationships(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestResetKeysSingleEntityNoRelationships()
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
                resetter.ResetKeysEntityAndRelationships(entity);

                //VERIFY 
                entity.BookId.ShouldEqual(0);
            }
        }

        [Fact]
        public void TestResetKeysSingleEntityWithRelationships()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();
                var entity = context.Books
                    .Include(x => x.Reviews)
                    .Include(x => x.Promotion)
                    .Include(x => x.AuthorsLink)
                        .ThenInclude(x => x.Author)
                    .First(x => x.Reviews.Any());

                //ATTEMPT
                var resetter = new DataResetter(context);
                resetter.ResetKeysEntityAndRelationships(entity);

                //VERIFY 
                entity.BookId.ShouldEqual(0);
                entity.Reviews.All(x => x.ReviewId == 0).ShouldBeTrue();
                entity.Reviews.All(x => x.BookId == 0).ShouldBeTrue();
                entity.Promotion.BookId.ShouldEqual(0);
                entity.Promotion.PriceOfferId.ShouldEqual(0);
                entity.AuthorsLink.All(x => x.AuthorId == 0).ShouldBeTrue();
                entity.AuthorsLink.All(x => x.BookId == 0).ShouldBeTrue();
                entity.AuthorsLink.Select(x => x.Author).All(x => x.AuthorId == 0).ShouldBeTrue();
            }
        }

        [Fact]
        public void TestResetKeysCollectionWithRelationships()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();
                var entities = context.Books
                    .Include(x => x.Reviews)
                    .Include(x => x.Promotion)
                    .Include(x => x.AuthorsLink)
                    .ThenInclude(x => x.Author)
                    .ToList();

                //ATTEMPT
                var resetter = new DataResetter(context);
                resetter.ResetKeysEntityAndRelationships(entities);

                //VERIFY 
                foreach (var entity in entities)
                {
                    entity.BookId.ShouldEqual(0);
                    entity.Reviews.All(x => x.ReviewId == 0).ShouldBeTrue();
                    entity.Reviews.All(x => x.BookId == 0).ShouldBeTrue();
                    if (entity.Promotion != null)
                    {
                        entity.Promotion.BookId.ShouldEqual(0);
                        entity.Promotion.PriceOfferId.ShouldEqual(0);
                    }
                    entity.AuthorsLink.All(x => x.AuthorId == 0).ShouldBeTrue();
                    entity.AuthorsLink.All(x => x.BookId == 0).ShouldBeTrue();
                    entity.AuthorsLink.Select(x => x.Author).All(x => x.AuthorId == 0).ShouldBeTrue();                   
                }
            }
        }


    }
}