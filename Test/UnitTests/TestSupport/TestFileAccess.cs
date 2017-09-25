using System.Linq;
using TestSupport.Helpers;
using Xunit.Extensions.AssertExtensions;
using Xunit;

namespace Test.UnitTests.TestSupport
{
    public class TestFileAccess
    {
        [Fact]
        public void TestGetCallerTopLevelDirectory()
        {
            //SETUP

            //ATTEMPT
            var path = TestFileHelpers.GetCallingAssemblyTopLevelDirectory();

            //VERIFY
            path.ShouldEndWith(this.GetType().Namespace.Split('.').First());
        }

        [Fact]
        public void TestGetTestDataFileDirectory()
        {
            //SETUP

            //ATTEMPT
            var path = TestFileHelpers.GetTestDataFileDirectory();

            //VERIFY
            path.ShouldEndWith(this.GetType().Namespace.Split('.').First() + "\\TestData");
        }

        [Fact]
        public void TestGetTestDataDummyFilePath()
        {
            //SETUP

            //ATTEMPT
            var path = TestFileHelpers.GetTestDataFilePath("Dummy*.txt");

            //VERIFY
            path.ShouldEndWith("\\TestData\\Dummy file.txt");
        }

        [Fact]
        public void TestGetTestDataDummyFilePathSubDirectory()
        {
            //SETUP

            //ATTEMPT
            var path = TestFileHelpers.GetTestDataFilePath(@"SubDirWithOneFileInIt\One file.txt");

            //VERIFY
            path.ShouldEndWith(@"SubDirWithOneFileInIt\One file.txt");
        }


        [Fact]
        public void TestGetTestDataAllFilesInDir()
        {
            //SETUP

            //ATTEMPT
            var filePaths = TestFileHelpers.GetPathFilesOfGivenName(@"*.*");

            //VERIFY
            filePaths.Length.ShouldNotEqual(0);
        }

        [Fact]
        public void TestGetTestDataDummyFileContext()
        {
            //SETUP

            //ATTEMPT
            var content = TestFileHelpers.GetTestDataFileContent("Dummy*.txt");

            //VERIFY
            content.ShouldEqual("This is the content of the dummy file");
        }

        [Fact]
        public void TestGetTestDataFileDirectoryWithRedirect()
        {
            //SETUP

            //ATTEMPT
            var path = TestFileHelpers.GetTestDataFileDirectory("..\\TestSupport");

            //VERIFY
            path.ShouldEndWith("\\TestSupport");
        }
    }
}
