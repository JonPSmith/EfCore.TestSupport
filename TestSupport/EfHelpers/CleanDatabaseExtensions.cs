// Copied from SqlServerDatabaseFacadeExtensions class and altered by GitHun @JonPSmith
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TestSupport.EfHelpers.Internal;
using TestSupport.Helpers;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// Static class holding the EnsureClean extension method
    /// </summary>
    public static class CleanDatabaseExtensions
    {
        /// <summary>
        /// Calling this will wipe the database schema down to nothing in the database and then calls
        /// Database.EnsureCreated to create the correct database schema based on your EF Core Context
        /// NOTE: This only works for SQL Server and PostgreSQL databases
        /// </summary>>
        /// <param name="databaseFacade">The Database property of the current DbContext that you want to clean</param>
        /// <param name="setUpSchema">Optional: by default it will set the schema to match the current DbContext configuration. If false leaves the database empty</param>
        public static void EnsureClean(this DatabaseFacade databaseFacade, bool setUpSchema = true)
        {
            if (databaseFacade.IsSqlServer())
                //SQL Server
                databaseFacade.CreateExecutionStrategy()
                    .Execute(databaseFacade, database => new SqlServerDatabaseCleaner(databaseFacade).Clean(database, setUpSchema));
            else if (databaseFacade.IsNpgsql())
            {
                //PostgreSQL

                //The databaseFacade doesn't have the Password, so we need to get it from the connection string in the appsettings.json file
                var config = AppSettings.GetConfiguration(Assembly.GetCallingAssembly());
                var password = new NpgsqlConnectionStringBuilder(config.GetConnectionString(AppSettings.PostgreSqlConnectionString)).Password;
                databaseFacade.FasterPostgreSqlEnsureClean(password, setUpSchema);
            }
            else
                throw new InvalidOperationException("The EnsureClean method only works with SQL Server or PostgreSQL databases.");
        }
    }
}