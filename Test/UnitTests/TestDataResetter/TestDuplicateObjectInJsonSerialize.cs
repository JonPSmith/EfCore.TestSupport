// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;

namespace Test.UnitTests.TestDataResetter
{
    public class TestDuplicateObjectInJsonSerialize
    {
        private readonly ITestOutputHelper _output;

        public TestDuplicateObjectInJsonSerialize(ITestOutputHelper output)
        {
            _output = output;
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
            var many3 = new ManyToMany();
            var many4 = new ManyToMany();
            var book1 = new TestBook(many1);
            var book2 = new TestBook(many2);
            var book3 = new TestBook(many3);
            var book4 = new TestBook(many4);
            var author1 = new TestAuthor { TestAuthorId = 1 };
            var author2 = new TestAuthor { TestAuthorId = 2 };
            var author3 = new TestAuthor { TestAuthorId = 3 };
            many1.SetBookAuthor(book1, author1);
            many2.SetBookAuthor(book2, author1);
            many3.SetBookAuthor(book3, author2);
            many4.SetBookAuthor(book4, author3);

            author1.Many = new List<ManyToMany> { many1, many2 };
            author2.Many = new List<ManyToMany> { many3 };
            author3.Many = new List<ManyToMany> { many4 };

            return new List<TestBook> { book1, book2, book3, book4 };
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

            public TestBook(ManyToMany many)
            {
                _many = new HashSet<ManyToMany>{many};
            }

            public int TestBookId { get; set; }
            public IEnumerable<ManyToMany> Many => _many.ToList();
        }

        public class TestAuthor
        {
            public int TestAuthorId { get; set; }
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