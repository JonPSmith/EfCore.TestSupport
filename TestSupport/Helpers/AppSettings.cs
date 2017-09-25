// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Data.SqlClient;
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
        public const string ConnectionStringName = "DefaultConnection";

        /// <summary>
        /// This will look for a appsettings.json file in the top level of the calling assembly and read content
        /// </summary>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration()
        {
            var callingProjectPath = TestFileHelpers.GetCallingAssemblyTopLevelDirectory();
            var builder = new ConfigurationBuilder()
                .SetBasePath(callingProjectPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            return builder.Build();
        }

        /// <summary>
        /// This creates a unique database name based on the test class name, and an optional extra name
        /// </summary>
        /// <returns></returns>
        public static string GetUniqueDatabaseConnectionString(this object testClass, string optionalMethodName = null)
        {
            var config = GetConfiguration();
            var orgConnect = config.GetConnectionString(ConnectionStringName);
            var builder = new SqlConnectionStringBuilder(orgConnect);

            var extraDatabaseName = $".{testClass.GetType().Name}";
            if (!string.IsNullOrEmpty(optionalMethodName)) extraDatabaseName += $".{optionalMethodName}";

            builder.InitialCatalog += extraDatabaseName;

            return builder.ToString();
        }
    }
}