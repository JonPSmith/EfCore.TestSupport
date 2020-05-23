// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace TestSupport.EfHelpers
{
    public class TimeThingResult
    {
        public TimeThingResult(double totalTimeMilliseconds, int numRuns, string message)
        {
            TotalTimeMilliseconds = totalTimeMilliseconds;
            NumRuns = numRuns;
            Message = message;
        }

        public double TotalTimeMilliseconds { get; private set; }
        public int NumRuns { get; private set; }
        public string Message { get; private set; }

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