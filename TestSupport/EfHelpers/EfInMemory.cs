// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// This static class contains extention methods to use with In-Memory databases
    /// </summary>
    public static class EfInMemory
    {
        /// <summary>
        /// This creates the options for an in-memory database with a unique name
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="throwOnClientServerWarning">Optional: default will throw exception if QueryClientEvaluationWarning is logged. Set to false if not needed</param>
        /// <returns></returns>
        public static DbContextOptions<TContext> CreateOptions<TContext>
            (bool throwOnClientServerWarning = true) where TContext : DbContext
        {
            return Guid.NewGuid().ToString().CreateOptions<TContext>(throwOnClientServerWarning);
        }

        /// <summary>
        /// This creates the options for an in-memory database, with the name given.
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="dbName">name of in-memory database</param>
        /// <param name="throwOnClientServerWarning">Optional: default will throw exception if QueryClientEvaluationWarning is logged. Set to false if not needed</param>
        /// <returns></returns>
        public static DbContextOptions<TContext> CreateOptions<TContext>
            (this string dbName, bool throwOnClientServerWarning = true)
            where TContext : DbContext
        {
            // Create a fresh service provider, and therefore a fresh 
            // InMemory database instance.
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Create a new options instance telling the context to use an
            // InMemory database and the new service provider.
            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseInMemoryDatabase(dbName)
                .UseInternalServiceProvider(serviceProvider);
            builder.ApplyOtherOptionSettings();

            return builder.Options;
        }

    }
}