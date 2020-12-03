// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace TestSupport.Helpers
{
    /// <summary>
    /// This is a static method that contains extension methods to get the configuation and form useful connection name strings
    /// </summary>
    public static class AppSettings
    {
        /// <summary>
        /// This is the default connection name that the AppSetting class expects
        /// </summary>
        public const string UnitTestConnectionStringName = "UnitTestConnection";

        /// <summary>
        /// Your unit test database name must end with this string.
        /// This is a safety measure to stop the DeleteAllUnitTestDatabases from deleting propduction databases
        /// </summary>
        public const string RequiredEndingToUnitTestDatabaseName = "Test";

        private const string AppSettingFilename = "appsettings.json";

        /// <summary>
        /// This will look for a appsettings.json file in the top level of the calling assembly and read content
        /// </summary>
        /// <param name="callingAssembly">If called by an internal method you must provide the other calling assembly</param>
        /// <param name="settingsFilename">This allows you to open a json configuration file of this given name</param>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration(Assembly callingAssembly = null, string settingsFilename = AppSettingFilename) //#A
        {
            var callingProjectPath =                      //#B
                TestData.GetCallingAssemblyTopLevelDir(callingAssembly ?? Assembly.GetCallingAssembly()); //#B
            var builder = new ConfigurationBuilder()               //#C
                .SetBasePath(callingProjectPath)                   //#C
                .AddJsonFile(settingsFilename, optional: true); //#C
            return builder.Build(); //#D
        }
        /******************************************************************
        #A This method returns an IConfigurationRoot, form which I can use methods, such as GetConnectionString("ConnectionName"), to access the configuration information
        #B In my TestSupport library I have a method that returns the absolute path of the calling assembly's top level directory. That will be the assembly that you are running your tests in
        #C I then use ASP.NET Core's ConfigurationBuilder to read that appsettings.json file. It is optional, so no error is thrown if the configuration file doesn’t exist
        #D Finally I call the Build() method, which returns the IConfigurationRoot type
         * ***************************************************************/

        /// <summary>
        /// This will look for a appsettings.json file in the directory relative to the calling assembly
        /// </summary>
        /// <param name="relativeToCallingAssembly">A relative path relative to the top level directory of the assembly you are calling from
        /// e.g. "..\MyAspNetApp" would get the appsettings.json from a project directory "MyAspNetApp" at the same level as your test assembly</param>
        /// <param name="settingsFilename">This allows you to open a json configuration file of this given name</param>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration(string relativeToCallingAssembly, string settingsFilename = AppSettingFilename) //#A
        {
            var callingProjectPath = TestData.GetCallingAssemblyTopLevelDir(Assembly.GetCallingAssembly());
            var pathToLookIn = Path.GetFullPath(callingProjectPath +"\\" + relativeToCallingAssembly);
            var builder = new ConfigurationBuilder()
                .SetBasePath(pathToLookIn)    
                .AddJsonFile(settingsFilename, optional: true);
            return builder.Build();
        }


        /// <summary>
        /// This creates a unique database name based on the test class name, and an optional extra name
        /// </summary>
        /// <param name="testClass">This should be 'this' in the test, which means the class name is added to the end of the database name</param>
        /// <param name="optionalMethodName">This is an optional string which, if present, is added to the end of the database name</param>
        /// <param name="seperator">Optional (defaults to _). This is the character used to separate each part of the formed name</param>
        /// <returns></returns>
        public static string GetUniqueDatabaseConnectionString(this object testClass, string optionalMethodName = null, char seperator = '_')
        {
            var config = GetConfiguration(Assembly.GetAssembly(testClass.GetType()));
            var orgConnect = config.GetConnectionString(UnitTestConnectionStringName);
            if (string.IsNullOrEmpty( orgConnect))
                throw new InvalidOperationException($"You are missing a connection string of name '{UnitTestConnectionStringName}' in the {AppSettingFilename} file.");
            var builder = new SqlConnectionStringBuilder(orgConnect);
            if (!builder.InitialCatalog.EndsWith(RequiredEndingToUnitTestDatabaseName))
                throw new InvalidOperationException($"The database name in your connection string must end with '{RequiredEndingToUnitTestDatabaseName}', but is '{builder.InitialCatalog}'."+
                    " This is a safety measure to help stop DeleteAllUnitTestDatabases from deleting production databases.");

            var extraDatabaseName = $"{seperator}{testClass.GetType().Name}";
            if (!string.IsNullOrEmpty(optionalMethodName)) extraDatabaseName += $"{seperator}{optionalMethodName}";

            builder.InitialCatalog += extraDatabaseName;

            return builder.ToString();
        }
    }
}