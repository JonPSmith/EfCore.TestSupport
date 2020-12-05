// Copied from SqlServerDatabaseFacadeExtensions class and altered by GitHun @JonPSmith
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TestSupport.EfHelpers.Internal;

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
        /// NOTE: This only works for SQL Server
        /// </summary>>
        /// <param name="databaseFacade">The Database property of the current DbContext that you want to clean</param>
        /// <param name="setUpSchema">Optional: by default it will set the schema to match the current DbContext configuration. If false leaves the database empty</param>
        public static void EnsureClean(this DatabaseFacade databaseFacade, bool setUpSchema = true)
        {
            if (!databaseFacade.IsSqlServer())
                throw new InvalidOperationException("The EnsureClean method only works with ");

            databaseFacade.CreateExecutionStrategy()
                .Execute(databaseFacade, database => new SqlServerDatabaseCleaner(databaseFacade).Clean(database, setUpSchema));
        }
    }
}