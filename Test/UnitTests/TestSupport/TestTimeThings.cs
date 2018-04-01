using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestSupport
{
    public class TestTimeThings
    {


        private class MockOutput : ITestOutputHelper
        {
            public string LastWriteLine { get; private set; }

            public void WriteLine(string message)
            {
                LastWriteLine = message;
            }

            public void WriteLine(string format, params object[] args)
            {
                throw new System.NotImplementedException();
            }
        }

        [Fact]                  
        public void TestNoSettings() 
        {
            //SETUP                  
            var mock = new MockOutput();

            //ATTEMPT
            using (new TimeThings(mock))
            {
                Thread.Sleep(10);
            }

            //VERIFY
            mock.LastWriteLine.ShouldStartWith(" took ");
            mock.LastWriteLine.ShouldEndWith("ms.");
        }

        [Fact]
        public void TestMessage()
        {
            //SETUP                  
            var mock = new MockOutput();

            //ATTEMPT
            using (new TimeThings(mock, "This message"))
            {
                Thread.Sleep(10);
            }

            //VERIFY
            mock.LastWriteLine.ShouldStartWith("This message took ");
            mock.LastWriteLine.ShouldEndWith("ms.");
        }

        [Fact]
        public void TestMessageAndNumRuns()
        {
            //SETUP                  
            var mock = new MockOutput();

            //ATTEMPT
            using (new TimeThings(mock, "This message", 500))
            {
                Thread.Sleep(10);
            }

            //VERIFY
            mock.LastWriteLine.ShouldStartWith("500 x This message took ");
            mock.LastWriteLine.ShouldEndWith("us.");
            mock.LastWriteLine.ShouldContain(", ave. per run = ");
        }

    }
}
