using DataLayer.BookApp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using TestSupport.DesignTimeServices;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.EfSchemaCompare
{
    public class TestDesignTimeServiceProvider
    {

        private readonly ITestOutputHelper _output;
        private readonly DbContextOptions<BookContext> _options;
        private readonly string _connectionString;
        public TestDesignTimeServiceProvider(ITestOutputHelper output)
        {
            _output = output;
            _options = this
                .CreateUniqueClassOptions<BookContext>();

            using (var context = new BookContext(_options))
            {
                _connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
            }
        }

        [Fact]
        public void GetDatabaseProvider()
        {
            //SETUP
            var optionsBuilder = new DbContextOptionsBuilder<BookContext>();
            optionsBuilder.UseSqlServer(_connectionString);

            using (var context = new BookContext(_options))
            {
                //ATTEMPT 
                var dbProvider = context.GetService<IDatabaseProvider>();

                //VERIFY
                dbProvider.ShouldNotBeNull();
                dbProvider.Name.ShouldEqual("Microsoft.EntityFrameworkCore.SqlServer");
            }
        }


        [Fact]
        public void GetDesignTimeServiceProviderSqlServer()
        {
            //SETUP
            using (var context = new BookContext(_options))
            {
                //ATTEMPT 
                var service = context.GetDesignTimeProvider();

                //VERIFY
                service.ShouldNotBeNull();
                service.ShouldBeType<ServiceProvider>();
            }
        }

        [Fact]
        public void GetDesignTimeServiceProviderSqlite()
        {
            //SETUP
            var options = SqliteInMemory
                .CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                //ATTEMPT 
                var service = context.GetDesignTimeProvider();

                //VERIFY
                service.ShouldNotBeNull();
                service.ShouldBeType<ServiceProvider>();
            }
        }

        [Fact]
        public void GetIDatabaseModelFactorySqlServer()
        {
            //SETUP
            using (var context = new BookContext(_options))
            {
                var serviceProvider = context.GetDesignTimeProvider();

                //ATTEMPT 
                var factory = serviceProvider.GetService<IDatabaseModelFactory>();

                //VERIFY
                factory.ShouldNotBeNull();
                factory.ShouldBeType<SqlServerDatabaseModelFactory>();
            }
        }

        [Fact]
        public void GetIDatabaseModelFactorySqlite()
        {
            //SETUP
            var options = SqliteInMemory
                .CreateOptions<BookContext>();
            using (var context = new BookContext(options))
            {
                var serviceProvider = context.GetDesignTimeProvider();

                //ATTEMPT 
                var factory = serviceProvider.GetService<IDatabaseModelFactory>();

                //VERIFY
                factory.ShouldNotBeNull();
                factory.ShouldBeType<SqliteDatabaseModelFactory>();
            }
        }

        [Fact]
        public void GetIScaffoldingModelFactory()
        {
            //SETUP
            var serviceProvider = DatabaseProviders.SqlServer.GetDesignTimeProvider();

            //ATTEMPT 
            var factory = serviceProvider.GetService<IScaffoldingModelFactory>();

            //VERIFY
            factory.ShouldNotBeNull();
            factory.ShouldBeType<RelationalScaffoldingModelFactory>();
        }
    }
}
