using System.Linq;
using DataLayer.MyEntityDb.EfCompareDbs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using TestSupport.DesignTimeServices;
using TestSupport.EfHelpers;
using TestSupport.EfSchemeCompare;
using TestSupport.EfSchemeCompare.Internal;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.EfSchemaCompare
{
    public class ComparerMyEntityDiff
    {
        private readonly ITestOutputHelper _output;
        private readonly string _connectionString;
        private readonly DbContextOptions<MyEntityDbContext> _options;
        private readonly DatabaseModel _databaseModel;
        public ComparerMyEntityDiff(ITestOutputHelper output)
        {
            _output = output;
            _options = this
                .CreateUniqueClassOptions<MyEntityDbContext>();
            var serviceProvider = DatabaseProviders.SqlServer.GetDesignTimeProvider();
            var factory = serviceProvider.GetService<IDatabaseModelFactory>();

            using (var context = new MyEntityDbContext(_options))
            {
                _connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
                _databaseModel = factory.Create(_connectionString, new string[] { }, new string[] { });
            }
        }

        [Fact]
        public void CompareDefaultConfigNoErrors()
        {
            //SETUP
            using (var context = new MyEntityDbContext(_options))
            {
                var model = context.Model;
                var handler = new DbContextComparer(model, context.GetType().Name);

                //ATTEMPT
                handler.CompareModelToDatabase(_databaseModel);

                //VERIFY
                CompareLog.HadErrors(handler.Logs).ShouldBeFalse();
            }
        }

        [Fact]
        public void CompareSchemaConfig()
        {
            //SETUP
            var optionsBuilder = new DbContextOptionsBuilder<MyEntitySetSchemaDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            using (var context = new MyEntitySetSchemaDbContext(optionsBuilder.Options))
            {
                var handler = new DbContextComparer(context.Model, context.GetType().Name);

                //ATTEMPT
                handler.CompareModelToDatabase(_databaseModel);

                //VERIFY
                CompareLog.HadErrors(handler.Logs).ShouldBeTrue();
                CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                    "NOT IN DATABASE: Entity 'MyEntity', table name. Expected = MySchema.MyEntities");
            }
        }

        [Fact]
        public void CompareExtraProperty()
        {
            //SETUP
            var optionsBuilder = new DbContextOptionsBuilder<MyEntityExtraPropDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            using (var context = new MyEntityExtraPropDbContext(optionsBuilder.Options))
            {
                var handler = new DbContextComparer(context.Model, context.GetType().Name);

                //ATTEMPT
                handler.CompareModelToDatabase(_databaseModel);

                //VERIFY
                CompareLog.HadErrors(handler.Logs).ShouldBeTrue();
                CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                    "NOT IN DATABASE: MyEntity->Property 'ShadowProp', column name. Expected = ShadowProp");
            }
        }

        [Fact]
        public void ComparePropertyDiffColName()
        {
            //SETUP
            var optionsBuilder = new DbContextOptionsBuilder<MyEntityPropertyDiffColDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            using (var context = new MyEntityPropertyDiffColDbContext(optionsBuilder.Options))
            {
                var handler = new DbContextComparer(context.Model, context.GetType().Name);

                //ATTEMPT
                handler.CompareModelToDatabase(_databaseModel);

                //VERIFY
                CompareLog.HadErrors(handler.Logs).ShouldBeTrue();
                CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                    "NOT IN DATABASE: MyEntity->Property 'MyInt', column name. Expected = OtherColName");
            }
        }

        [Fact]
        public void ComparePropertyDiffNullName()
        {
            //SETUP
            var optionsBuilder = new DbContextOptionsBuilder<MyEntityPropertyDiffNullDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            using (var context = new MyEntityPropertyDiffNullDbContext(optionsBuilder.Options))
            {
                var handler = new DbContextComparer(context.Model, context.GetType().Name);

                //ATTEMPT
                handler.CompareModelToDatabase(_databaseModel);

                //VERIFY
                CompareLog.HadErrors(handler.Logs).ShouldBeTrue();
                CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyString', nullability. Expected = NOT NULL, Found = NULL");
            }
        }

        [Fact]
        public void ComparePropertyDiffTypeName()
        {
            //SETUP
            var optionsBuilder = new DbContextOptionsBuilder<MyEntityPropertyDiffTypeDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            using (var context = new MyEntityPropertyDiffTypeDbContext(optionsBuilder.Options))
            {
                var handler = new DbContextComparer(context.Model, context.GetType().Name);

                //ATTEMPT
                handler.CompareModelToDatabase(_databaseModel);

                //VERIFY
                CompareLog.HadErrors(handler.Logs).ShouldBeTrue();
                CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyString', column type. Expected = varchar(max), Found = nvarchar(max)");
            }
        }

        [Fact]
        public void ComparePropertyComputedColName()
        {
            //SETUP
            var optionsBuilder = new DbContextOptionsBuilder<MyEntityComputedColDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            using (var context = new MyEntityComputedColDbContext(optionsBuilder.Options))
            {
                var handler = new DbContextComparer(context.Model, context.GetType().Name);

                //ATTEMPT
                handler.CompareModelToDatabase(_databaseModel);

                //VERIFY
                CompareLog.HadErrors(handler.Logs).ShouldBeTrue();
                var errors = CompareLog.ListAllErrors(handler.Logs).ToList();
                errors.Count.ShouldEqual(2);
                errors[0].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyDateTime', computed column sql. Expected = getutcdate(), Found = <null>");
                errors[1].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyDateTime', value generated. Expected = OnAddOrUpdate, Found = Never");
            }
        }

        [Fact]
        public void ComparePropertyComputedColNameReversed()
        {
            //SETUP
            var serviceProvider = DatabaseProviders.SqlServer.GetDesignTimeProvider();
            var factory = serviceProvider.GetService<IDatabaseModelFactory>();
            var options = this.CreateUniqueMethodOptions<MyEntityComputedColDbContext>();
            DatabaseModel localDatabaseModel;
            using (var context = new MyEntityComputedColDbContext(options))
            {
                var connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
                localDatabaseModel = factory.Create(connectionString, new string[] { }, new string[] { });
            }

            using (var context = new MyEntityDbContext(_options))
            {
                var handler = new DbContextComparer(context.Model, context.GetType().Name);

                //ATTEMPT
                handler.CompareModelToDatabase(localDatabaseModel);

                //VERIFY
                CompareLog.HadErrors(handler.Logs).ShouldBeTrue();
                var errors = CompareLog.ListAllErrors(handler.Logs).ToList();
                errors.Count.ShouldEqual(3);
                errors[0].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyDateTime', column type. Expected = datetime2, Found = datetime");
                errors[1].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyDateTime', computed column sql. Expected = <null>, Found = (getutcdate())");
                errors[2].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyDateTime', value generated. Expected = Never, Found = OnAddOrUpdate");
            }
        }

        [Fact]
        public void ComparePropertySqlDefaultName()
        {
            //SETUP
            var optionsBuilder = new DbContextOptionsBuilder<MyEntitySqlDefaultDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            using (var context = new MyEntitySqlDefaultDbContext(optionsBuilder.Options))
            {
                var handler = new DbContextComparer(context.Model, context.GetType().Name);

                //ATTEMPT
                handler.CompareModelToDatabase(_databaseModel);

                //VERIFY
                CompareLog.HadErrors(handler.Logs).ShouldBeTrue();
                var errors = CompareLog.ListAllErrors(handler.Logs).ToList();
                errors.Count.ShouldEqual(2);
                errors[0].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyInt', default value sql. Expected = 123, Found = <null>");
                errors[1].ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyInt', value generated. Expected = OnAdd, Found = Never");
            }
        }

        [Fact]
        public void ComparePropertySqlDefaultReversed()
        {
            //SETUP
            var serviceProvider = DatabaseProviders.SqlServer.GetDesignTimeProvider();
            var factory = serviceProvider.GetService<IDatabaseModelFactory>();
            var options = this.CreateUniqueMethodOptions<MyEntitySqlDefaultDbContext>();
            DatabaseModel localDatabaseModel;
            using (var context = new MyEntitySqlDefaultDbContext(options))
            {
                var connectionString = context.Database.GetDbConnection().ConnectionString;
                context.Database.EnsureCreated();
                localDatabaseModel = factory.Create(connectionString, new string[] { }, new string[] { });
            }

            using (var context = new MyEntityDbContext(_options))
            {
                var handler = new DbContextComparer(context.Model, context.GetType().Name);

                //ATTEMPT
                handler.CompareModelToDatabase(localDatabaseModel);

                //VERIFY
                CompareLog.HadErrors(handler.Logs).ShouldBeTrue();
                CompareLog.ListAllErrors(handler.Logs).Single().ShouldEqual(
                    "DIFFERENT: MyEntity->Property 'MyInt', default value sql. Expected = <null>, Found = ((123))");
            }
        }

    }
}
