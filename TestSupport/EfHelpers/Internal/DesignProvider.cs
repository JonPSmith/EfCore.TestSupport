// // Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;

#pragma warning disable EF1001 // Internal EF Core API usage.
namespace TestSupport.EfHelpers.Internal
{
    /// <summary>
    /// This static class contains the methods to return a design-time service provider
    /// </summary>
    internal static class DesignProvider
    {
        private const string SqlServerProviderName = "Microsoft.EntityFrameworkCore.SqlServer";
        private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";


        /// <summary>
        /// This returns the correct instance of the design time service for the current DbContext's Database property
        /// </summary>
        /// <param name="databaseFacade"></param>
        /// <returns></returns>
        public static IDesignTimeServices GetDesignTimeService(this DatabaseFacade databaseFacade)
        {
            if (databaseFacade.IsSqlServer())
                //Only handles SQL Server
                return new SqlServerDesignTimeServices();
            else if (databaseFacade.IsNpgsql())
                //Only handles SQL Server
                return new NpgsqlDesignTimeServices();

            throw new InvalidOperationException("This is not a database provider that we currently support.");
        }

        /// <summary>
        /// This returns a DesignTimeProvider for the design time service instance that you provided
        /// </summary>
        /// <param name="designTimeService">This should be an instance of rhe design time service for the database provider</param>
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