// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TestSupport.Helpers;

namespace TestSupport.EfHelpers
{
    public static class SqlServerOptions
    {
        public static DbContextOptions<T> CreateUniqueClassOptions<T>(this object callingClass, 
            bool throwOnClientServerWarning = false) where T : DbContext
        {
            var connectionString = callingClass.GetUniqueDatabaseConnectionString();
            var builder = new DbContextOptionsBuilder<T>();

            builder.UseSqlServer(connectionString)
                .EnableSensitiveDataLogging();  //You get more information with this turned on.
            builder.CheckAddThrowOnClientServerWarning(throwOnClientServerWarning);

            return builder.Options;
        }

        public static DbContextOptions<T> CreateUniqueMethodOptions<T>(this object callingClass,
            bool throwOnClientServerWarning = false,
            [CallerMemberName] string callingMember = "") where T : DbContext
        {
            var connectionString = callingClass.GetUniqueDatabaseConnectionString(callingMember);
            var builder = new DbContextOptionsBuilder<T>();

            builder.UseSqlServer(connectionString)
                .EnableSensitiveDataLogging();  //You get more information with this turned on.
            builder.CheckAddThrowOnClientServerWarning(throwOnClientServerWarning);

            return builder.Options;
        }
    }
}