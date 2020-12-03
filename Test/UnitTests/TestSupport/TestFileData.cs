using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TestSupport.Helpers;
using Xunit.Extensions.AssertExtensions;
using Xunit;

namespace Test.UnitTests.TestSupport
{
    public class TestFileData
    {
        [Fact]
        public void TestGetCallerTopLevelDirectory()
        {
            //SETUP

            //ATTEMPT
            var path = TestData.GetCallingAssemblyTopLevelDir();

            //VERIFY
            path.ShouldEndWith(this.GetType().Namespace.Split('.').First());
        }
        

        [Fact]
        public void TestGetTestDataFileDirectory()
        {
            //SETUP

            //ATTEMPT
            var path = TestData.GetTestDataDir();

            //VERIFY
            path.ShouldEndWith(this.GetType().Namespace.Split('.').First() + "\\TestData");
        }

        [Fact]
        public void TestGetTestDataDummyFilePath()
        {
            //SETUP

            //ATTEMPT
            var path = TestData.GetFilePath("Dummy*.txt");

            //VERIFY
            path.ShouldEndWith("\\TestData\\Dummy file.txt");
        }

        [Fact]
        public void TestGetTestDataDummyFilePathSubDirectory()
        {
            //SETUP

            //ATTEMPT
            var path = TestData.GetFilePath(@"SubDirWithOneFileInIt\One file.txt");

            //VERIFY
            path.ShouldEndWith(@"SubDirWithOneFileInIt\One file.txt");
        }

        [Fact]
        public void TestGetTestDataDummyFileAltTestDataDir()
        {
            //SETUP

            //ATTEMPT
            var path = TestData.GetFilePath(@"\AltTestDataDir\Alt dummy file.txt");

            //VERIFY
            path.ShouldEndWith(@"\AltTestDataDir\Alt dummy file.txt");
        }


        [Fact]
        public void TestGetTestDataAllFilesInDir()
        {
            //SETUP

            //ATTEMPT
            var filePaths = TestData.GetFilePaths(@"*.*");

            //VERIFY
            filePaths.Length.ShouldNotEqual(0);
        }

        [Fact]
        public void TestGetTestDataDummyFileContext()
        {
            //SETUP

            //ATTEMPT
            var content = TestData.GetFileContent("Dummy*.txt");

            //VERIFY
            content.ShouldEqual("This is the content of the dummy file");
        }

        [Fact]
        public void TestGetTestDataFileDirectoryWithRedirect()
        {
            //SETUP

            //ATTEMPT
            var path = TestData.GetTestDataDir("..\\TestSupport");

            //VERIFY
            path.ShouldEndWith("\\TestSupport");
        }
    }
}
