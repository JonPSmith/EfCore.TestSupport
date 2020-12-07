// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Data.SqlClient;
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
        public void GetConfigurationDefiningTheJsonFileNameOk()
        {
            //SETUP

            //ATTEMPT
            var config = AppSettings.GetConfiguration(settingsFilename: "appsettings.json");

            //VERIFY
            config.GetConnectionString(AppSettings.UnitTestConnectionStringName)
                .ShouldEqual("Server=(localdb)\\mssqllocaldb;Database=EfCore.TestSupport-Test;Trusted_Connection=True;MultipleActiveResultSets=true");
        }

        [Fact]
        public void GetConfigurationFromDifferentAssemblyOk()
        {
            //SETUP

            //ATTEMPT
            var config = AppSettings.GetConfiguration("..\\DataLayer");
            var myData = config["MyString"];

            //VERIFY
            myData.ShouldEqual("This is in the DataLayer");
        }

        [Fact]
        public void GetConfigurationFromLowerLevelWithDifferentNameOk()
        {
            //SETUP

            //ATTEMPT
            var config = AppSettings.GetConfiguration("\\TestData", "differentAppSettings.json");
            var myData = config["MyString"];

            //VERIFY
            myData.ShouldEqual("This is in the TestData directory in Test");
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
            newDatabaseName.ShouldEqual ($"{orgDbName}_{GetType().Name}");
        }

        [Fact]
        public void GetTestConnectionStringDifferentSeperatorOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();
            var orgDbName = new SqlConnectionStringBuilder(config.GetConnectionString(AppSettings.UnitTestConnectionStringName)).InitialCatalog;

            //ATTEMPT
            var con = this.GetUniqueDatabaseConnectionString(null, '.');

            //VERIFY
            var newDatabaseName = new SqlConnectionStringBuilder(con).InitialCatalog;
            newDatabaseName.ShouldEqual($"{orgDbName}.{GetType().Name}");
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
            newDatabaseName.ShouldEqual($"{orgDbName}_{typeof(TestAppSettings).Name}_ExtraMethodName");
        }

        [Fact]
        public void GetMyIntOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();

            //ATTEMPT
            var myData = config["MyInt"];

            //VERIFY
            myData.ShouldEqual("1");
        }

        [Fact]
        public void GetMyInnerIntOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();

            //ATTEMPT
            var myData = config["MyObject:MyInnerInt"];

            //VERIFY
            myData.ShouldEqual("2");
        }
    }
}