// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Test.Helpers;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using Xunit.Abstractions;

namespace Test.UnitCommands
{
    public class DeleteAllUnitTestDatabases
    {
        private readonly ITestOutputHelper _output;

        public DeleteAllUnitTestDatabases(ITestOutputHelper output)
        {
            _output = output;
        }

        //Run this method to wipe ALL the SQL Server test databases using your appsetting.json connection string
        //You need to run it in debug mode - that stops it being run when you "run all" unit tests
        [RunnableInDebugOnly]  //#A
        public void DeleteAllSqlServerTestDatabasesOk() //#B
        {
            var numDeleted = DatabaseTidyHelper //#C
                .DeleteAllSqlServerTestDatabases();//#C
            _output.WriteLine(                              //#D
                "This deleted {0} SQL Server databases.", numDeleted); //#D
        }

        //Run this method to wipe ALL the PostgreSql test databases using your appsetting.json connection string
        //You need to run it in debug mode - that stops it being run when you "run all" unit tests
        [RunnableInDebugOnly]  //#A
        public void DeleteAllPostgreSqlTestDatabasesOk()
        {
            var numDeleted = DatabaseTidyHelper
                .DeleteAllPostgreSqlTestDatabases();
            _output.WriteLine( 
                "This deleted {0} PostgreSql databases.", numDeleted);
        }
    }
}
