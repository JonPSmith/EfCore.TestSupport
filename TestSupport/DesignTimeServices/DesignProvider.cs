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
    /// This static class contains the methods to return a design-time service provider
    /// </summary>
    public static class DesignProvider
    {
        private const string SqlServerProviderName = "Microsoft.EntityFrameworkCore.SqlServer";
        private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";

        /// <summary>
        /// This returns the correct instance of the design time service for the current DbContext
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IDesignTimeServices GetDesignTimeService(this DbContext context)
        {
            var dbProvider = context.GetService<IDatabaseProvider>();
            if (dbProvider == null)
                throw new InvalidOperationException("Cound not find a database provider service.");

            var providerName = dbProvider.Name;

            if (providerName == SqlServerProviderName)
                return new SqlServerDesignTimeServices();
            if (providerName == SqliteProviderName)
                return new SqliteDesignTimeServices();

            throw new InvalidOperationException("This is not a database provider that we currently support.");
        }

        /// <summary>
        /// This returns a DesignTimeProvider for the design time service instance that you provided
        /// </summary>
        /// <param name="designTimeService">This should be an instance of rhe design time service for the database provider</param>
        /// <returns></returns>
        public static ServiceProvider GetDesignTimeProvider(this IDesignTimeServices designTimeService)
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

            designTimeService.ConfigureDesignTimeServices(serviceCollection);
            return serviceCollection.BuildServiceProvider();
        }
    }
}