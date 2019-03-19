
using TestSupport.Helpers;
using Xunit;
using Xunit.Extensions;
using Xunit.Extensions.AssertExtensions;

namespace Net472Test
{

    public class TestAccessDirectory
    {
        [Fact]
        public void TestGetTestDataDir()
        {
            //SETUP

            //ATTEMPT
            var dataDir = TestData.GetTestDataDir();

            //VERIFY
            dataDir.ShouldEndWith("TestData");
        }

        [Fact]
        public void GetMyIntOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();

            //ATTEMPT
            var myData = config["MyInt"];

            //VERIFY
            myData.ShouldEqual("1");
        }

        [Fact]
        public void GetMyInnerIntOk()
        {
            //SETUP
            var config = AppSettings.GetConfiguration();

            //ATTEMPT
            var myData = config["MyObject:MyInnerInt"];

            //VERIFY
            myData.ShouldEqual("2");
        }
    }
}
