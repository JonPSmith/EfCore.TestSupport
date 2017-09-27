// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestSupport
{
    public class TestAppSettings
    {
        [Fact]
        public void GetConfigurationOk()
        {
            //SETUP

            //ATTEMPT
            var config = AppSettings.GetConfiguration();

            //VERIFY
            config.GetConnectionString(AppSettings.UnitTestConnectionStringName)
                .ShouldEqual("Server=(localdb)\\mssqllocaldb;Database=EfCore.TestSupport-Test;Trusted_Connection=True;MultipleActiveResultSets=true");
        }

        [Fact]
        public void GetTestConnectionStringOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var orgDbName = new SqlConnectionStringBuilder(config.GetConnectionString(AppSettings.UnitTestConnectionStringName)).InitialCatalog;

            //ATTEMPT
            var con = this.GetUniqueDatabaseConnectionString();

            //VERIFY
            var newDatabaseName = new SqlConnectionStringBuilder(con).InitialCatalog;
            newDatabaseName.ShouldEqual ($"{orgDbName}.{typeof(TestAppSettings).Name}");
        }


        [Fact]
        public void GetTestConnectionStringWithExtraMethodNameOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var orgDbName = new SqlConnectionStringBuilder(config.GetConnectionString(AppSettings.UnitTestConnectionStringName)).InitialCatalog;

            //ATTEMPT
            var con = this.GetUniqueDatabaseConnectionString("ExtraMethodName");

            //VERIFY
            var newDatabaseName = new SqlConnectionStringBuilder(con).InitialCatalog;
            newDatabaseName.ShouldEqual($"{orgDbName}.{typeof(TestAppSettings).Name}.ExtraMethodName");
        }
    }
}