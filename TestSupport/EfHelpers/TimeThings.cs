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
        private Action<TimeThingResult> _funcToCall;
        private readonly ITestOutputHelper _output;
        private readonly string _message;
        private readonly int _numRuns;

        /// <summary>
        /// This will measure the time it took from this class being created to it being disposed
        /// </summary>
        /// <param name="message"></param>
        /// <param name="numRuns">Optional: if the timing covers multiple runs of something, then set numRuns to the number of runs and it will give you the average per run</param>
        public TimeThings(Action<TimeThingResult> result, string message = "", int numRuns = 0)
            : this(message, numRuns)
        {
            _funcToCall = result;
        }

        /// <summary>
        /// This will measure the time it took from this class being created to it being disposed and writes out to xUnit ITestOutputHelper
        /// </summary>
        /// <param name="output"></param>
        /// <param name="message"></param>
        /// <param name="numRuns">Optional: if the timing covers multiple runs of something, then set numRuns to the number of runs and it will give you the average per run</param>
        public TimeThings(ITestOutputHelper output, string message = "", int numRuns = 0)
            : this(message, numRuns)
        {
            _output = output;
        }

        private TimeThings(string message = "", int numRuns = 0)
        {
            _message = message;
            _numRuns = numRuns;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        /// <summary>
        /// When disposed it will return the result, either via a action or by an output
        /// </summary>
        public void Dispose()
        {
            _stopwatch.Stop();
            var timeMilliseconds = _stopwatch.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
            var result = new TimeThingResult(timeMilliseconds, _numRuns, _message);
            _funcToCall?.Invoke(result);
            _output?.WriteLine(result.ToString());
        }


    }
}