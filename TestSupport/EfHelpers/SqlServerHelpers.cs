// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        /// <param name="addBracketsAndSchema">Optional: normally it only uses the table name, but for cases where you have multiple schemas,
        /// or a table name that needs brackets the you can set to to true. Deafult is false</param>
        /// <param name="maxDepth">Valuse to stop the wipe method from getting in a circular refence loop</param>
        /// <param name="excludeTypes">This allows you to provide the Types of the table that you don't want wiped. 
        /// Useful if you have a circular ref that WipeAllDataFromDatabase cannot handle. You then must wipe that part.</param>
        /// <returns>True if the database is created, false if it already existed.</returns>
        public static bool CreateEmptyViaWipe(this DbContext context,
            bool addBracketsAndSchema = false,
            int maxDepth = 10, params Type[] excludeTypes)
        {
            var databaseWasCreated = context.Database.EnsureCreated();
            if (!databaseWasCreated)
                context.WipeAllDataFromDatabase(addBracketsAndSchema, maxDepth, excludeTypes);
            return databaseWasCreated;
        }

        /// <summary>
        /// This creates the DbContextOptions  options for a SQL server database, 
        /// where the database name is formed using the appsetting's DefaultConnection with the class name and the calling method's name as as a prefix.
        /// That is, the database is unique to the calling method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="throwOnClientServerWarning">Optional: default will throw exception if QueryClientEvaluationWarning is logged. Set to false if not needed</param>
        /// <param name="callingMember">Do not use: this is filled in by compiler</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueMethodOptions<T>(this object callingClass,
            bool throwOnClientServerWarning = true,
            [CallerMemberName] string callingMember = "") where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass, throwOnClientServerWarning, callingMember);
        }

        /// <summary>
        /// This creates the DbContextOptions  options for a SQL server database, 
        /// where the database name is formed using the appsetting's DefaultConnection with the class name as a prefix.
        /// That is, the database is unique to the object provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="throwOnClientServerWarning">Optional: default will throw exception if QueryClientEvaluationWarning is logged. Set to false if not needed</param>
        /// <returns></returns>
        public static DbContextOptions<T>
            CreateUniqueClassOptions<T>( //#A
                this object callingClass, //#B
                bool throwOnClientServerWarning = true) //#C
            where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>         //#D
                (callingClass, throwOnClientServerWarning);//#D
        }

        //------------------------------------
        //private methods

        private static DbContextOptions<T> 
            CreateOptionWithDatabaseName<T>(  //#E
                object callingClass,            //#F
                bool throwOnClientServerWarning,//#F 
                string callingMember = null)    //#F
            where T : DbContext
        {
            var connectionString = callingClass     //#G
                .GetUniqueDatabaseConnectionString( //#G
                    callingMember);                 //#G
            var builder =                          //#H
                new DbContextOptionsBuilder<T>();  //#H
            builder.UseSqlServer(connectionString);//#H
            builder.ApplyOtherOptionSettings(      //#I
                throwOnClientServerWarning);       //#I

            return builder.Options; //#J
        }

        internal static void ApplyOtherOptionSettings<T>(this DbContextOptionsBuilder<T> builder, 
                bool throwOnClientServerWarning) 
            where T : DbContext
        {
            builder.EnableSensitiveDataLogging();
            if (throwOnClientServerWarning)
            {
                builder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
            }
        }
        /*******************************************************************
        #A This extension method will return options for a SQL Server database with a name starting with database name in the original connection string in the appsettings.json file, but with the name of the class of the instance provided in the first parameter
        #B It is expected that the object instance provided will be 'this', that is, the class in which the unit test is running
        #C By default it will throw an exception if a QueryClientEvaluationWarning is logged. You can turn this off if you do not want that to happen
        #D I now call a private method that is shared between this method and the CreateUniqueMethodOptions options
        #E This is the methods that builds the SQL Server part of the options, with the correct database name
        #F These parameters are passed from the CreateUniqueClassOptions. For CreateUniqueClassOptions the calling method is left as null
        #G This is the AppSettings's extension method. It will return the connection string from the appsetting.json file, but with the database name modified with the callingClass's type name as a suffix
        #H This sets up the OptionsBuilder and creates a SQL Server database provider with the connection string
        #I I how call a general method used on all my option builders. This enables sensitive logging so that you get more information, and, if the throwOnClientServerWarning is true, it configures the warning to throw on a QueryClientEvaluationWarning being logged
         * ******************************************************************/
    }
}