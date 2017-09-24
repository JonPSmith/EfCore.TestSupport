// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using TestSupport.Helpers;

namespace TestSupport.EfHelpers
{
    public static class SqlServerOptions
    {
        public static DbContextOptions<T> CreateUniqueClassOptions<T>(this object callingClass, 
            bool throwOnClientServerWarning = false) where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, throwOnClientServerWarning);
        }

        public static DbContextOptions<T> CreateUniqueMethodOptions<T>(this object callingClass,
            bool throwOnClientServerWarning = false,
            [CallerMemberName] string callingMember = "") where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, throwOnClientServerWarning, callingMember);
        }

        //------------------------------------
        //private methods

        private static DbContextOptions<T> CreateOptionWithDatabaseName<T>(object callingClass, 
            bool throwOnClientServerWarning, string callingMember = null) where T : DbContext
        {
            var connectionString = callingClass.GetUniqueDatabaseConnectionString(callingMember);
            var builder = new DbContextOptionsBuilder<T>();

            builder.UseSqlServer(connectionString);
            builder.ApplyOtherOptionSettings(throwOnClientServerWarning);

            return builder.Options;
        }
    }
}