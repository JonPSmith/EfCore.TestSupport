// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using TestSupport.Helpers;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// This static class contains extention methods to use with SQL Server databases
    /// </summary>
    public static class SqlServerHelpers
    {
        /// <summary>
        /// This will ensure an empty database by deleting the current database and recreating it
        /// </summary>
        /// <param name="context"></param>
        public static void CreateEmptyViaDelete(this DbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        /// <summary>
        /// This will ensure an empty database by using the WipeAllDataFromDatabase method
        /// </summary>
        /// <param name="context"></param>
        /// <param name="maxDepth">Valuse to stop the wipe method from getting in a circular refence loop</param>
        /// <param name="excludeTypes">This allows you to provide the Types of the table that you don't want wiped. 
        /// Useful if you have a circular ref that WipeAllDataFromDatabase cannot handle. You then must wipe that part.</param>
        public static void CreateEmptyViaWipe(this DbContext context, int maxDepth = 10, params Type[] excludeTypes)
        {
            if (!context.Database.EnsureCreated())
                context.WipeAllDataFromDatabase(maxDepth, excludeTypes);
        }

        /// <summary>
        /// This creates the DbContextOptions  options for a SQL server database, 
        /// where the database name is formed using the appsetting's DefaultConnection with the class name as a prefix.
        /// That is, the database is unique to the object provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="throwOnClientServerWarning">Optional: if set to true then will throw exception if QueryClientEvaluationWarning is logged</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueClassOptions<T>(this object callingClass, 
            bool throwOnClientServerWarning = false) where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, throwOnClientServerWarning);
        }

        /// <summary>
        /// This creates the DbContextOptions  options for a SQL server database, 
        /// where the database name is formed using the appsetting's DefaultConnection with the class name and the calling method's name as as a prefix.
        /// That is, the database is unique to the calling method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="throwOnClientServerWarning">Optional: if set to true then will throw exception if QueryClientEvaluationWarning is logged</param>
        /// <param name="callingMember">Do not use: this is filled in by compiler</param>
        /// <returns></returns>
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