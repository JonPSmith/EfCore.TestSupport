// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestSupport.DesignTimeServices;
using TestSupport.EfSchemeCompare.Internal;
using TestSupport.Helpers;

namespace TestSupport.EfSchemeCompare
{
    /// <summary>
    /// This is the main class for Comparing EF Core DbContexts againsts a database to see if they differ
    /// </summary>
    public class CompareEfSql
    {
        private readonly List<CompareLog> _logs = new List<CompareLog>();
        /// <summary>
        /// This returns a single string containing all the errors found
        /// Each error is on a separate line
        /// </summary>
        public string GetAllErrors => string.Join("\n", CompareLog.ListAllErrors(Logs));

        /// <summary>
        /// This gives you access to the full log. but its not an easy thing to parse
        /// Look at the CompareLog class for various static methods that will output the log in a human-readable format
        /// </summary>
        public IReadOnlyList<CompareLog> Logs => _logs.ToImmutableList();

        /// <summary>
        /// This will compare a single DbContext against a database. It returns true if there were errors
        /// </summary>
        /// <param name="context">This must be a valid instance of a DbContext</param>
        /// <param name="configOrConnectionString">Optional: If provided it should either be a 
        /// connection string or the name of a connection string in the appsetting.json file.
        /// If null, then it uses the connection string in your DbContext</param>
        /// <returns></returns>
        public bool CompareEfWithDb(DbContext context, string configOrConnectionString = null)
        {
            var stage1Comparer = new Stage1Comparer(context.Model, context.GetType().Name, _logs);
            var databaseModel = GetDatabaseModelViaScaffolder(context, configOrConnectionString);
            var hasErrors = stage1Comparer.CompareModelToDatabase(databaseModel);
            if (hasErrors) return true;

            //No errors, so its worth running the second phase
            var stage2Comparer = new Stage2Comparer(databaseModel);
            hasErrors = stage2Comparer.CompareLogsToDatabase(_logs);
            return hasErrors;
        }

        //------------------------------------------------------
        //private methods

        private static DatabaseModel GetDatabaseModelViaScaffolder(DbContext context, string configOrConnectionString)
        {
            var serviceProvider = context.GetDesignTimeProvider();
            var factory = serviceProvider.GetService<IDatabaseModelFactory>();
            var connectionString = configOrConnectionString == null
                ? context.Database.GetDbConnection().ConnectionString
                : GetConfigurationOrActualString(configOrConnectionString);

            return factory.Create(connectionString, new string[] { }, new string[] { });
        }

        private static string GetConfigurationOrActualString(string configOrConnectionString)
        {
            var config = AppSettings.GetConfiguration();
            var connectionFromConfigFile = config.GetConnectionString(configOrConnectionString);
            return connectionFromConfigFile ?? configOrConnectionString;
        }
    }
}