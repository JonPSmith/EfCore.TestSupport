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

        //Run this method to wipe ALL the test database for the current branch
        //You need to run it in debug mode - that stops it being run when you "run all" unit tests
        [RunnableInDebugOnly]  //#A
        public void DeleteAllTestDatabasesOk() //#B
        {
            var numDeleted = SqlAdoNetHelpers //#C
                .DeleteAllUnitTestDatabases();//#C
            _output.WriteLine(                              //#D
                "This deleted {0} databases.", numDeleted); //#D
        }
        /****************************************************************
        #A The [RunnableInDebugOnly] makes sure the unit command is not run by accident when the main unit tests are run. I must manually run this method in debug mode
        #B This has the format of a unit test, that is a public method which returns void
        #C I call the DeleteAllUnitTestBranchDatabases method from my EcCore.TestSupport library. This returns the number of databases that it deleted
        #D I then write out how many database were deleted by this method
         * ****************************************************************/
    }
}
