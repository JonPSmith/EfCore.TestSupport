// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Data.SqlClient;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace TestSupport.Helpers
{
    public static class AppSettings
    {
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
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetUniqueDatabaseConnectionString<T>(this T testClass, string optionalMethodName = null)
        {
            var config = GetConfiguration();
            var orgConnect = config.GetConnectionString(ConnectionStringName);
            var builder = new SqlConnectionStringBuilder(orgConnect);

            var extraDatabaseName = $".{typeof(T).Name}";
            if (optionalMethodName != null) extraDatabaseName += $".{optionalMethodName}";

            builder.InitialCatalog += extraDatabaseName;

            return builder.ToString();
        }
    }
}