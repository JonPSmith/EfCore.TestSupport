// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Data.SqlClient;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TestSupport.Helpers;

namespace TestSupport.SeedDatabase
{
    /// <summary>
    /// This provides a way to set up the options for opening a SQL production database
    /// </summary>
    /// <typeparam name="YourDbContext"></typeparam>
    public class SqlServerProductionSetup<YourDbContext> where YourDbContext : DbContext
    {
        /// <summary>
        /// This provides the options 
        /// </summary>
        public DbContextOptions<YourDbContext> Options { get; }

        /// <summary>
        /// This provides the name of the database that was opened. Useful if you want to save the data using the database name
        /// </summary>
        public string DatabaseName { get; }

        /// <summary>
        /// This sets up the Options and DatabaseName properties ready for you to open the SQL database
        /// </summary>
        /// <param name="connectionNameOrConnectionString">This is either the name of a connection in the appsetting.json file or the actual connection string </param>
        public SqlServerProductionSetup(string connectionNameOrConnectionString)
        {
            var connection = GetConfigurationOrActualString(connectionNameOrConnectionString, Assembly.GetCallingAssembly());
            var builder = new SqlConnectionStringBuilder(connection);
            DatabaseName = builder.InitialCatalog;
            var optionsBuilder = new DbContextOptionsBuilder<YourDbContext>();
            optionsBuilder.UseSqlServer(connection);
            Options = optionsBuilder.Options;
        }

        //--------------------------------------------
        //private methods

        private string GetConfigurationOrActualString(string configOrConnectionString, Assembly callingAssembly)
        {
            var config = AppSettings.GetConfiguration(callingAssembly);
            var connectionFromConfigFile = config.GetConnectionString(configOrConnectionString);
            return connectionFromConfigFile ?? configOrConnectionString;
        }
    }
}