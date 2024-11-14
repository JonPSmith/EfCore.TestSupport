// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information..

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// This used to internal EF Core code that was a quick reset of a database.
    /// In .NET9 the internal code changed, and I decided to go back to the normal
    /// EnsureDeleted / EnsureCreated. This means your existing tests will still work.
    /// </summary>
    public static class CleanDatabaseExtensions
    {
        /// <summary>
        /// Calling this will call EnsureDeleted and then EnsureCreated.
        /// This works with any database supported be EF Core
        /// </summary>>
        /// <param name="databaseFacade">The Database property of the current DbContext that you want to clean</param>
        public static void EnsureClean(this DatabaseFacade databaseFacade)
        {
            databaseFacade.EnsureDeleted();
            databaseFacade.EnsureCreated();
        }
    }
}