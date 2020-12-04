// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Diagnostics;
using Xunit;

namespace TestSupport.Attributes
{
    /// <summary>
    /// Useful attribute for stopping a test from being run
    /// see https://lostechies.com/jimmybogard/2013/06/20/run-tests-explicitly-in-xunit-net/ 
    /// </summary>
    public class RunnableInDebugOnlyAttribute : FactAttribute
    {
        /// <summary>
        /// By putting this attribute on a test instead of the normal [Fact] attribute will mean the 
        /// test will only run if in debug mode.
        /// This is useful for stopping unit tests that should not be run in the normal run of unit tests
        /// </summary>
        public RunnableInDebugOnlyAttribute()
        {
            if (!Debugger.IsAttached)
            {
                Skip = "Only running in interactive mode.";
            }
        }
    }
}