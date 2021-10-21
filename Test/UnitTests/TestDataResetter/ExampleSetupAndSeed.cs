// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer.BookApp;
using DataLayer.EfCode.BookApp;
using Microsoft.EntityFrameworkCore;
using RandomNameGeneratorLibrary;
using Test.Helpers;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using TestSupport.SeedDatabase;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataResetter
{
    public class ExampleSetupAndSeed 
    {
        private readonly ITestOutputHelper _output;

        public ExampleSetupAndSeed(ITestOutputHelper output)
        {
            _output = output;
        }

        [RunnableInDebugOnly(DisplayName = "Needs database XYZ")]
        public void ExampleSetup()
        {
            //This simulates the production database
            //Typically you would get the options for a database using the SqlProductionSetup class.
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                //1a. Read in the data to want to seed the database with
                var entities = context.Books
                    .Include(x => x.Reviews)
                    .Include(x => x.Promotion)
                    .Include(x => x.AuthorsLink)
                        .ThenInclude(x => x.Author)
                    .ToList();

                //1b. Reset primary and foreign keys (see next version for anonymise)
                var resetter = new DataResetter(context);
                resetter.ResetKeysEntityAndRelationships(entities);

                //1c. Convert to JSON string
                var jsonString = entities.DefaultSerializeToJson();
                //1d. Save to JSON local file in TestData directory
                "ExampleDatabase".WriteJsonToJsonFile(jsonString);
            }
        }

        /// <summary>
        /// This is an example of a method that can override the default anonymiser to use more useful replacement names
        /// NOTE: This version does not obey any Max or Min options.
        /// </summary>
        public class MyAnonymiser
        {
            readonly PersonNameGenerator _pGenerator;

            /// <summary>
            /// Creates the Anonymiser
            /// </summary>
            /// <param name="seed">If not given then random sequence,
            /// or if number given then same sequence every time</param>
            public MyAnonymiser(int? seed = null)
            {
                var random = seed == null ? new Random() : new Random((int)seed);
                _pGenerator = new PersonNameGenerator(random);
            }

            public string AnonymiseThis(AnonymiserData data, object objectInstance)
            {
                switch (data.ReplacementType)
                {
                    case "FullName": return _pGenerator.GenerateRandomFirstAndLastName();
                    case "FirstName": return _pGenerator.GenerateRandomFirstName();
                    case "LastName": return _pGenerator.GenerateRandomLastName();
                    case "Email":
                        return
                            $"{_pGenerator.GenerateRandomFirstName()}.{_pGenerator.GenerateRandomLastName()}@gmail.com";
                    default: return _pGenerator.GenerateRandomFirstAndLastName();
                }
            }
        }

        [RunnableInDebugOnly(DisplayName = "Needs database XYZ")]
        public void ExampleSetupWithAnonymiseLinkToLibrary()
        {
            //SETUP

            //This simulates the production database
            //Typically you would open a database 
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();

                //1a. Read in the data to want to seed the database with
                var entities = context.Books
                    .Include(x => x.Reviews)
                    .Include(x => x.Promotion)
                    .Include(x => x.AuthorsLink)
                        .ThenInclude(x => x.Author)
                    .ToList();

                //1b-ii. Set up resetter config to use own method
                var myAnonymiser = new MyAnonymiser(42);
                var config = new DataResetterConfig
                {
                    AnonymiserFunc = myAnonymiser.AnonymiseThis
                };
                //1b-ii. Add all class/properties that you want to anonymise
                config.AddToAnonymiseList<Author>(x => x.Name, "FullName");
                config.AddToAnonymiseList<Review>(x => x.VoterName, "FirstName");
                //1b. Reset primary and foreign keys and anonymise author's name and Review's VoterName
                var resetter = new DataResetter(context, config);
                resetter.ResetKeysEntityAndRelationships(entities);

                //Show author's myAnonymiser
                foreach (var author in entities.SelectMany(x => x.AuthorsLink.Select(y => y.Author)).Distinct())
                {
                    _output.WriteLine($"Author name = {author.Name}");
                }

                //1c. Convert to JSON string
                var jsonString = entities.DefaultSerializeToJson();
                //1d. Save to JSON local file in TestData directory
                "ExampleDatabaseAnonymised".WriteJsonToJsonFile(jsonString);
            }
        }

        [Fact]
        public void TestWhatHappensIfDoNotResetPkFk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            Book entity;
            using (var context = new BookContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();
                entity = context.Books
                    .Include(x => x.AuthorsLink)
                    .ThenInclude(x => x.Author)
                    .First();
            }
            using (var context = new BookContext(options))
            {
                //ATTEMPT
                context.Add(entity);
                var ex = Assert.Throws<DbUpdateException>(() => context.SaveChanges());

                //VERIFY 
                ex.InnerException.Message.ShouldEqual("SQLite Error 19: 'UNIQUE constraint failed: Authors.AuthorId'.");
            }
        }

        [Fact]
        public void ExampleSeedDatabase()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                //2a. make sure you have an empty database
                context.Database.EnsureCreated();
                //2b. read the entities back from the JSON file
                var entities = "ExampleDatabase".ReadSeedDataFromJsonFile<List<Book>>();
                //2c. Optionally “tweak” any specific data in the classes that your unit test needs
                entities.First().Title = "new title";
                //2d. Add the data to the database and save
                context.AddRange(entities);
                context.SaveChanges();

                //ATTEMPT
                //... run your tests here

                //VERIFY 
                context.Books.First().Title.ShouldEqual("new title");
                context.Books.Count().ShouldEqual(4);
                context.Authors.Count().ShouldEqual(3);
            }
        }

    }
}