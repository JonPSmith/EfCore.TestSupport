// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Xunit.Abstractions;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// Use this in a using statement for timing things - time output to test output when class is disposes
    /// </summary>
    public class TimeThings : IDisposable
    {
        private readonly Stopwatch _stopwatch ;
        private readonly ITestOutputHelper _output;
        private readonly string _message;

        /// <summary>
        /// This will measure the time it took from this class being created to it being disposed
        /// </summary>
        /// <param name="output"></param>
        /// <param name="message"></param>
        public TimeThings(ITestOutputHelper output, string message = "")
        {
            _output = output;
            _message = message;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        /// <summary>
        /// When disposed it will write to the test output the time it took from creation
        /// </summary>
        public void Dispose()
        {
            _stopwatch.Stop();
            var timeMilliseconds = _stopwatch.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
            _output?.WriteLine($"{_message} took {timeMilliseconds:#,###.00} ms.");
        }
    }
}