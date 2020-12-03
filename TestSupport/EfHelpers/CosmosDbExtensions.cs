// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace TestSupport.EfHelpers
{

    /// <summary>
    /// Extensions to provide Cosmos DB options where the Database/Container is held in the Azure Cosmos DB Emulator.
    /// </summary>
    public static class CosmosDbExtensions
    {
        /// <summary>
        /// This creates Cosmos DB options where the Database/Container is held in the Azure Cosmos DB Emulator.
        /// It uses the name of the object as the database name (normally "this", which is your unit test class.
        /// For method-level uniqueness set makeMethodUnique to true to add the calling method to end of the database name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callingClass">this object is used to get the name of the database</param>
        /// <param name="makeMethodUnique">optional: If set then add the calling method's name to the end of the object name.</param>
        /// <param name="callingMember"></param>
        /// <returns></returns>
        public static DbContextOptions<T> GetCosmosDbToEmulatorOptions<T>(this object callingClass,
            bool makeMethodUnique = false, [CallerMemberName] string callingMember = "") where T : DbContext
        {
            var databaseName = callingClass.GetType().Name;
            if (makeMethodUnique)
                databaseName += callingMember;
            return databaseName.GetCosmosDbToEmulatorOptions<T>();
        }

        /// <summary>
        /// This creates Cosmos DB options where the Database/Container is held in the Azure Cosmos DB Emulator.
        /// The given string will be the database name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="databaseName">name to use for database name.</param>
        /// <returns></returns>
        public static DbContextOptions<T> GetCosmosDbToEmulatorOptions<T>(this string databaseName) where T : DbContext
        {
            var builder = new DbContextOptionsBuilder<T>()
                .UseCosmos(
                    "https://localhost:8081",
                    "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                    databaseName);
            return builder.Options;
        }
    }
}