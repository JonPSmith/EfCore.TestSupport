// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace TestSupport.DesignTimeServices
{
    /// <summary>
    /// This defines the database providers that the DesignProvider currently supports
    /// </summary>
    public enum DatabaseProviders { SqlServer, Sqlite}

    /// <summary>
    /// This static class contains the methods to return a design-time service provider
    /// </summary>
    public static class DesignProvider
    {
        private const string SqlServerProviderName = "Microsoft.EntityFrameworkCore.SqlServer";
        private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";

        /// <summary>
        /// This returns design-time service provider for the database provider you are using in the DbContext
        /// </summary>
        /// <param name="context"></param>
        /// <returns>A service provider that conatins the design-time services</returns>
        public static ServiceProvider GetDesignTimeProvider(this DbContext context)
        {
            return GetDesignTimeProvider(context.DecodeDatabaseProvider());
        }

        /// <summary>
        /// This returns design-time service provider for the database provider you have selected from the enum
        /// </summary>
        /// <param name="databaseProvider"></param>
        /// <returns>A service provider that conatins the design-time services</returns>
        public static ServiceProvider GetDesignTimeProvider(this DatabaseProviders databaseProvider)
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var reporter = new OperationReporter(
                new OperationReportHandler(
                    m => errors.Add(m),
                    m => warnings.Add(m)));

            // Add base services for scaffolding
            var serviceCollection = new ServiceCollection()
                .AddScaffolding(reporter)
                .AddSingleton<IOperationReporter, OperationReporter>()
                .AddSingleton<IOperationReportHandler, OperationReportHandler>();

            switch (databaseProvider)
            {
                case DatabaseProviders.SqlServer:
                {
                    var designProvider = new SqlServerDesignTimeServices();
                    designProvider.ConfigureDesignTimeServices(serviceCollection);
                    return serviceCollection.BuildServiceProvider();
                }
                case DatabaseProviders.Sqlite:
                {
                    var designProvider = new SqliteDesignTimeServices();
                    designProvider.ConfigureDesignTimeServices(serviceCollection);
                    return serviceCollection.BuildServiceProvider();
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, null);
            }
        }

        //----------------------------------------------
        //private methods

        private static DatabaseProviders DecodeDatabaseProvider(this DbContext context)
        {
            var dbProvider = context.GetService<IDatabaseProvider>();
            if (dbProvider == null)
                throw new InvalidOperationException("Cound not find a database provider service.");

            var providerName = dbProvider.Name;

            if (providerName == SqlServerProviderName)
                return DatabaseProviders.SqlServer;
            if (providerName == SqliteProviderName)
                return DatabaseProviders.Sqlite;

            throw new InvalidOperationException("This is not a database provider that we currently support.");

        }
    }
}