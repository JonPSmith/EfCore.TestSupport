// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TestSupport.EfHelpers
{
    public static class EfInMemory
    {
        public static DbContextOptions<TContext> CreateNewContextOptions<TContext>() where TContext : DbContext
        {
            return Guid.NewGuid().ToString().CreateNewContextOptions<TContext>();
        }

        public static DbContextOptions<TContext> CreateNewContextOptions<TContext>(this string dbName)
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

            return builder.Options;
        }

    }
}