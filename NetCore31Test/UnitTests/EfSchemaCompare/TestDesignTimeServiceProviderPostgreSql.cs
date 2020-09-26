using DataLayer.EfCode.BookApp;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NetCore31Test.Fixtures;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;
using TestSupport.DesignTimeServices;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace NetCore31Test.UnitTests.EfSchemaCompare
{
    public class TestDesignTimeServiceProviderPostgreSql : IClassFixture<PostgreSqlDatabaseFixture<BookContext>>
    {
        private readonly BookContext _bookContext;

        public TestDesignTimeServiceProviderPostgreSql(PostgreSqlDatabaseFixture<BookContext> fixture)
        {
            _bookContext = fixture.DbContext;
        }

        [Fact]
        public void GetDatabaseProviderPostgreSql()
        {
            // Act.
            var dbProvider = _bookContext.GetService<IDatabaseProvider>();

            // Assert.
            dbProvider.ShouldNotBeNull();
            dbProvider.Name.ShouldEqual("Npgsql.EntityFrameworkCore.PostgreSQL");
        }

        [Fact]
        public void GetDesignTimeServiceProviderPostgreSql()
        {
            // Act. 
            var service = _bookContext.GetDesignTimeService();

            // Assert.
            service.ShouldNotBeNull();
            service.ShouldBeType<NpgsqlDesignTimeServices>();
        }

        [Fact]
        public void GetIDatabaseModelFactoryPostgreSql()
        {
            // Arrange.
            var dtService = _bookContext.GetDesignTimeService();
            var serviceProvider = dtService.GetDesignTimeProvider();

            // Act.
            var factory = serviceProvider.GetService<IDatabaseModelFactory>();

            // Assert.
            factory.ShouldNotBeNull();
            factory.ShouldBeType<NpgsqlDatabaseModelFactory>();
        }
    }
}