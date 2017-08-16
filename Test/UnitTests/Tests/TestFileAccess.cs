using System.Linq;
using TestSupport.Helpers;
using Xunit.Extensions.AssertExtensions;
using Xunit;

namespace Test.UnitTests.Tests
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
