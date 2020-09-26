﻿// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;

namespace TestSupport.DesignTimeServices
{
    /// <summary>
    /// This static class contains the methods to return a design-time service provider
    /// </summary>
    public static class DesignProvider
    {
        private const string SqlServerProviderName = "Microsoft.EntityFrameworkCore.SqlServer";
        private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";
        private const string NpgsqlProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";

        /// <summary>
        /// This returns the correct instance of the design time service for the current DbContext
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IDesignTimeServices GetDesignTimeService(this DbContext context)
        {
            var dbProvider = context.GetService<IDatabaseProvider>();
            if (dbProvider == null)
                throw new InvalidOperationException("Could not find a database provider service.");

            var providerName = dbProvider.Name;

            if (providerName == SqlServerProviderName)
                return new SqlServerDesignTimeServices();
            if (providerName == SqliteProviderName)
                return new SqliteDesignTimeServices();
            if (providerName == NpgsqlProviderName)
                return new NpgsqlDesignTimeServices();

            throw new InvalidOperationException("This is not a database provider that we currently support.");
        }

        /// <summary>
        /// This returns a DesignTimeProvider for the design time service instance that you provided
        /// </summary>
        /// <param name="designTimeService">This should be an instance of the design time service for the database provider</param>
        /// <returns></returns>
        public static ServiceProvider GetDesignTimeProvider(this IDesignTimeServices designTimeService)
        {
            // Add base services for scaffolding
            var serviceCollection = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices()
                .AddSingleton<IOperationReporter, OperationReporter>()
                .AddSingleton<IOperationReportHandler, OperationReportHandler>();

            designTimeService.ConfigureDesignTimeServices(serviceCollection);
            return serviceCollection.BuildServiceProvider();
        }
    }
}