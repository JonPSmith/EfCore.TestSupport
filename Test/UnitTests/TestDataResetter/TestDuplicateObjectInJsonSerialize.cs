// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataResetter
{
    public class TestDuplicateObjectInJsonSerialize
    {
        private readonly ITestOutputHelper _output;

        public TestDuplicateObjectInJsonSerialize(ITestOutputHelper output)
        {
            _output = output;
        }

        //see https://github.com/JamesNK/Newtonsoft.Json/issues/2066 for discussion on how Json.Net compares 
        [Fact]
        public void TestEqualsDatabase()
        {
            //SETUP
            var entities = GetLinkEntities();

            //ATTEMPT
            var firstBookAuthor = entities.First().Many.First().AuthorLink;
            var reverseAuthorLink = entities.First().Many.First().AuthorLink.Many;
            var secondBookAuthor = entities.Last().Many.First().AuthorLink;

            //VERIFY
            entities.First().Many.Count().ShouldEqual(1);
            reverseAuthorLink.Count().ShouldEqual(2);
            entities.Last().Many.Count().ShouldEqual(1);
            firstBookAuthor.Equals(secondBookAuthor).ShouldBeTrue();
            reverseAuthorLink.First().BookLink.Equals(entities.First()).ShouldBeTrue();
            ReferenceEquals(firstBookAuthor, secondBookAuthor).ShouldBeTrue();
            reverseAuthorLink.Last().BookLink.Equals(entities.Last()).ShouldBeTrue();
        }

        [Fact]
        public void TestJsonSerializeNotInDatabase()
        {
            //SETUP
            var entities = GetLinkEntities();

            //ATTEMPT
            var json = JsonConvert.SerializeObject(entities, new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            });

            //FAILS WITH EXCEPTION
            _output.WriteLine(json);
        }

        [Fact]
        public void TestJsonSerializeNotInDatabaseReferenceLoopHandlingIgnore()
        {
            //SETUP
            var entities = GetLinkEntities();

            //ATTEMPT
            var json = JsonConvert.SerializeObject(entities, new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            });

            _output.WriteLine(json);
        }

        [Fact]
        public void TestJsonSerializeFromDatabaseReferenceLoopHandlingIgnore()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<TestDbContext>();
            using (var context = new TestDbContext(options))
            {
                context.Database.EnsureCreated();

                var seed = GetLinkEntities();
                context.AddRange(seed);
                context.SaveChanges();

                var entities = context.Books
                    .Include(x => x.Many).ThenInclude(x => x.AuthorLink);

                //ATTEMPT
                var json = JsonConvert.SerializeObject(entities, new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented
                });

                //VERIFY
                _output.WriteLine(json);
            }
        }

        private List<TestBook> GetLinkEntities()
        {
            var many1 = new ManyToMany();
            var many2 = new ManyToMany();
            var book1 = new TestBook("Book1", many1);
            var book2 = new TestBook("Book2", many2);
            var author1 = new TestAuthor("Author");
            many1.SetBookAuthor(book1, author1);
            many2.SetBookAuthor(book2, author1);

            author1.Many = new List<ManyToMany> { many1, many2 };

            return new List<TestBook> { book1, book2};
        }

        //--------------------------------------------------------------
        //EF Core stuff

        public class TestDbContext : DbContext
        {
            public TestDbContext(
                DbContextOptions<TestDbContext> options)
                : base(options) { }

            public DbSet<TestBook> Books { get; set; }
            public DbSet<TestAuthor> Authors { get; set; }

            protected override void
                OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ManyToMany>().HasKey(x => new {x.TestBookId, x.TestAuthorId});
            }
        }

        public class TestBook
        {
            private TestBook() {}

            private readonly HashSet<ManyToMany> _many;

            public TestBook(string title, ManyToMany many)
            {
                Title = title;
                _many = new HashSet<ManyToMany>{many};
            }

            public int TestBookId { get; set; }
            public string Title { get; set; }
            public IEnumerable<ManyToMany> Many => _many.ToList();
        }

        public class TestAuthor
        {
            public TestAuthor(string name)
            {
                Name = name;
            }

            public int TestAuthorId { get; set; }

            public string Name { get; set; }
            public ICollection<ManyToMany> Many { get; set; }
        }

        public class ManyToMany
        {
            public int TestBookId { get; private set; }
            public int TestAuthorId { get; private set; }
            public TestBook BookLink { get; private set; }
            public TestAuthor AuthorLink { get; private set; }

            public void SetBookAuthor(TestBook book, TestAuthor author)
            {
                BookLink = book;
                AuthorLink = author;
            }
        } 
    }
}