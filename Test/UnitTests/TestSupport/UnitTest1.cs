using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestSupport
{
    public class UnitTest1
    {
        [Fact]                  //#A
        public void DemoTest() //#B
        {
            //SETUP                  
            const int someValue = 1; //#C

            //ATTEMPT
            var result = someValue * 2; //#D

            //VERIFY
            result.ShouldEqual(2); //#E
        }
        /*****************************************************
        #A The [Fact] attribute tells the unit test runner that this method is an xUnit unit test that should be run
        #B The method must be public. It should return void, or if you are running async methods, then it should return "async Task" 
        #C Typically you put code here that sets up the data and/or environment for the unit test
        #D This is where you run the code you want to test
        #E And here is where you put the test(s) to check that the result of your test is correct
         * ***************************************************/
    }
}
