using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules.Databases;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace NetCore31Test.Fixtures
{
    public class PostgreSqlDatabaseFixture<T> : IAsyncLifetime where T : DbContext
    {
        public T DbContext;

        private PostgreSqlTestcontainer PostgreSqlTestContainer { get; set; }

        public async Task InitializeAsync()
        {
            var testContainersBuilder = new TestcontainersBuilder<PostgreSqlTestcontainer>()
                .WithDatabase(new PostgreSqlTestcontainerConfiguration
                {
                    Database = "db",
                    Username = "postgres",
                    Password = "postgres",
                });

            // Use specific Docker endpoint from env variable, if it exists.
            // Handy when running in special environments, as Bitbucket Pipelines.
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOCKER_HOST")))
            {
                testContainersBuilder = testContainersBuilder.WithDockerEndpoint(Environment.GetEnvironmentVariable("DOCKER_HOST"));
            }

            PostgreSqlTestContainer = testContainersBuilder.Build();
            await PostgreSqlTestContainer.StartAsync();

            // Init DB context.
            var builder = new DbContextOptionsBuilder<T>();
            builder.UseNpgsql(PostgreSqlTestContainer.ConnectionString);
            DbContext = (T) Activator.CreateInstance(typeof(T), builder.Options);

            if (DbContext == null)
                throw new InvalidOperationException("TODO");

            await DbContext.Database.EnsureDeletedAsync();

            // Apply EF Core migrations.
            await DbContext.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await PostgreSqlTestContainer.DisposeAsync();
        }
    }
}
