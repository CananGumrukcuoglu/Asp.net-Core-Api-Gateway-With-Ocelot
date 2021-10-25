using System.Diagnostics;

namespace Services.Common.Utilities
{
    public class TimeMeasurement
    {
        public static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }
    }
}
