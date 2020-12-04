// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// Result from a TimeThings instance once it is disposed
    /// </summary>
    public class TimeThingResult
    {
        /// <summary>
        /// Creates the TimeThingResult
        /// </summary>
        /// <param name="totalTimeMilliseconds"></param>
        /// <param name="numRuns"></param>
        /// <param name="message"></param>
        public TimeThingResult(double totalTimeMilliseconds, int numRuns, string message)
        {
            TotalTimeMilliseconds = totalTimeMilliseconds;
            NumRuns = numRuns;
            Message = message;
        }

        /// <summary>
        /// Total time in milliseconds, with fractions
        /// </summary>
        public double TotalTimeMilliseconds { get; private set; }

        /// <summary>
        /// Optional number of runs. zero if not set.
        /// </summary>
        public int NumRuns { get; private set; }

        /// <summary>
        /// Optional string to identify this usage of the TimeThings
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Provides a detailed report of the timed event
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var prefix = NumRuns > 1 ? $"{NumRuns:#,###} x " : "";
            var suffix = NumRuns > 1 ? $", ave. per run = {TimeScaled(TotalTimeMilliseconds / NumRuns)}" : "";
            return $"{prefix}{Message} took {TotalTimeMilliseconds:#,###.00} ms.{suffix}";
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