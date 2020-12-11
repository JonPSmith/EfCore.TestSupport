// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers.Internal;

namespace TestSupport.EfHelpers
{

    /// <summary>
    /// Extensions to provide Cosmos DB options where the Database/Container is held in the Azure Cosmos DB Emulator.
    /// </summary>
    public static class CosmosDbExtensions
    {
        /// <summary>
        /// This creates Cosmos DB options where the Database/Container is held in the Azure Cosmos DB Emulator.
        /// The name of the database is taken from the callingClass 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this object is used to get the name of the database</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueClassCosmosDbEmulator<T>(this object callingClass, Action<DbContextOptionsBuilder<T>> builder = null) where T : DbContext
        {
            return CreateOptionWithDatabaseName<T>(callingClass.GetType().Name, builder).Options;
        }

        /// <summary>
        /// This creates the DbContextOptions options for a Azure Cosmos DB Emulator database while capturing EF Core's logging output. 
        /// The name of the database is the type name of the callingClass parameter (normally this)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this should be this, i.e. the class you are in</param>
        /// <param name="logAction">This action is called with each log output</param>
        /// <param name="logToOptions">Optional: This allows you to define what logs you want and what format. Defaults to LogLevel.Information</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueClassCosmosDbEmulatorWithLogTo<T>(this object callingClass,
            Action<string> logAction,
            LogToOptions logToOptions = null, Action<DbContextOptionsBuilder<T>> builder = null)
            where T : DbContext
        {
            if (logAction == null) throw new ArgumentNullException(nameof(logAction));

            return CreateOptionWithDatabaseName(callingClass.GetType().Name, builder)
                .AddLogTo(logAction, logToOptions)
                .Options;
        }

        /// <summary>
        /// This creates Cosmos DB options where the Database/Container is held in the Azure Cosmos DB Emulator.
        /// The name of the database is the type name of the callingClass parameter (normally this) plus the method name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this object is used to get the name of the database</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <param name="callingMember">Do not use: this is filled in by compiler</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueMethodCosmosDbEmulator<T>(this object callingClass,
            Action<DbContextOptionsBuilder<T>> builder = null, [CallerMemberName] string callingMember = "") where T : DbContext
        {
            var databaseName = callingClass.GetType().Name;
            
                databaseName += callingMember;
            return databaseName.CreateOptionWithDatabaseName<T>(builder).Options;
        }

        /// <summary>
        /// This creates the DbContextOptions options for a Azure Cosmos DB Emulator database while capturing EF Core's logging output. 
        /// The name of the database is the type name of the callingClass parameter (normally this) plus the method name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this object is used to get the name of the database</param>
        /// <param name="logAction">This action is called with each log output</param>
        /// <param name="logToOptions">Optional: This allows you to define what logs you want and what format. Defaults to LogLevel.Information</param>
        /// <param name="builder">Optional: action that allows you to add extra options to the builder</param>
        /// <param name="callingMember">Do not use: this is filled in by compiler</param>
        /// <returns></returns>
        public static DbContextOptions<T> CreateUniqueMethodCosmosDbEmulatorWithLogTo<T>(this object callingClass,
            Action<string> logAction,
            LogToOptions logToOptions = null, Action<DbContextOptionsBuilder<T>> builder = null,
            [CallerMemberName] string callingMember = "") where T : DbContext
        {
            var databaseName = callingClass.GetType().Name;

            databaseName += callingMember;
            return databaseName.CreateOptionWithDatabaseName<T>(builder).Options;
        }

        //------------------------------------------
        //private methods

        private static DbContextOptionsBuilder<T> CreateOptionWithDatabaseName<T>(this string databaseName, Action<DbContextOptionsBuilder<T>> builder) 
            where T : DbContext
        {
            var localBuilder = new DbContextOptionsBuilder<T>()
                .UseCosmos(
                    "https://localhost:8081",
                    "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                    databaseName);
            localBuilder.ApplyOtherOptionSettings();
            builder?.Invoke(localBuilder);
            return localBuilder;
        }
    }
}