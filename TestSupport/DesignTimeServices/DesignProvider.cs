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
    public enum DatabaseProviders { SqlServer, Sqlite}

    public static class DesignProvider
    {
        private const string SqlServerProviderName = "Microsoft.EntityFrameworkCore.SqlServer";

        public static ServiceProvider GetDesignTimeProvider(this DbContext context)
        {
            return GetDesignTimeProvider(context.DecodeDatabaseProvider());
        }

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

            throw new InvalidOperationException("This is not a database provider that we currently support.");

        }
    }
}