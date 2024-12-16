// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestSupport;

public class TestTimeThings(ITestOutputHelper output)
{
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

    [Fact]
    public void TestTimeThingResultReturn()
    {
        //SETUP                  
        TimeThingResult result = null;

        //ATTEMPT
        using (new TimeThings(x => result = x, "This message", 10))
        {
            Thread.Sleep(10);
        }

        //VERIFY
        result.Message.ShouldEqual("This message");
        result.NumRuns.ShouldEqual(10);
        result.TotalTimeMilliseconds.ShouldBeInRange(10, 50);
    }

    [Fact]
    public void TestHowLongTimeThings()
    {
        //SETUP
        TimeThingResult result = null;

        //ATTEMPT
        using (new TimeThings(output, "TimeThings", 2))
        {
            using (new TimeThings(x => result = x))
            {

            }
        }

        //VERIFY
    }


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
}