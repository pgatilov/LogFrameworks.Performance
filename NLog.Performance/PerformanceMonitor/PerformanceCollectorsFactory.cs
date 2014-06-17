using System.Collections.Generic;

namespace NLog.Performance.PerformanceMonitor
{
    internal class PerformanceCollectorsFactory
    {
        public static IEnumerable<IPerformanceCollector> CreateCollectors()
        {
            yield return new TotalTimeElapsedCollector();
        }
    }
}