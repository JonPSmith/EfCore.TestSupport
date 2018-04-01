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
        private readonly int _numRuns;

        /// <summary>
        /// This will measure the time it took from this class being created to it being disposed
        /// </summary>
        /// <param name="output"></param>
        /// <param name="message"></param>
        /// <param name="numRuns">Optional: if the timing covers multiple runs of soemthing, then set numRuns to the number of runs and it will give you the average per run</param>
        public TimeThings(ITestOutputHelper output, string message = "", int numRuns = 0)
        {
            _output = output;
            _message = message;
            _numRuns = numRuns;
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
            var prefix = _numRuns > 1 ? $"{_numRuns:#,###} x " : "";
            var suffix = _numRuns > 1 ? $", ave. per run = {TimeScaled(timeMilliseconds/_numRuns)}" : "";
            _output?.WriteLine($"{prefix}{_message} took {timeMilliseconds:#,###.00} ms.{suffix}");
        }

        private string TimeScaled(double timeMilliseconds)
        {
            if (timeMilliseconds > 5 * 1000)
                return $"{timeMilliseconds / 1000:F3} sec.";
            if (timeMilliseconds > 5)
                return $"{timeMilliseconds:#,###.00} ms.";
            if (timeMilliseconds > 5 / 1000.0)
                return $"{timeMilliseconds * 1000:#,###.00} us.";
            return $"{timeMilliseconds * 1000_000:#,###.0} ns.";
        }
    }
}