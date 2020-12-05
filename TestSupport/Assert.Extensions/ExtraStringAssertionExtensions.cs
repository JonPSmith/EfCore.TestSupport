// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

#pragma warning disable 1591
namespace Xunit.Extensions.AssertExtensions
{
    /// <summary>
    /// Extra AssertExtensions that I find useful
    /// </summary>
    public static class ExtraStringAssertionExtensions
    {

        public static void ShouldStartWith(this string actualString,
            string expectedStartString)
        {
            Assert.StartsWith(expectedStartString, actualString);
        }

        public static void ShouldEndWith(this string actualString,
            string expectedEndString)
        {
            Assert.EndsWith(expectedEndString, actualString);
        }
    }
}